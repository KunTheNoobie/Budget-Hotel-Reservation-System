using Assignment.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

// ========== APPLICATION ENTRY POINT ==========
// This is the main entry point for the ASP.NET Core MVC application.
// This file (Program.cs) configures:
// 1. Services (database, authentication, email, etc.)
// 2. Database initialization and migrations
// 3. HTTP request pipeline (middleware, routing, etc.)
// This is where the application starts when you run it.

var builder = WebApplication.CreateBuilder(args);

// ========== 1. CONFIGURE SERVICES ==========
// Services are registered here and can be injected into controllers, services, etc.
// Services are created once per HTTP request (scoped) or once per application (singleton).

// ========== DATABASE CONFIGURATION ==========
// Configure database connection using Entity Framework Core
// Connection string is read from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        // ========== RETRY POLICY ==========
        // Enable automatic retry on database connection failures
        // This makes the application more resilient to temporary database issues
        // Retries up to 5 times with maximum 30 seconds delay between retries
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,                              // Try up to 5 times
            maxRetryDelay: TimeSpan.FromSeconds(30),       // Wait up to 30 seconds between retries
            errorNumbersToAdd: null);                      // Retry on all transient errors
    }));

// ========== ENCRYPTION SERVICE INITIALIZATION ==========
// Initialize Encryption Service for encrypting sensitive data (e.g., phone numbers)
// Encryption key is read from appsettings.json (must be at least 32 characters for AES-256)
// This MUST be called before the application starts to ensure encryption works
var encryptionKey = builder.Configuration["EncryptionKey"];
if (string.IsNullOrEmpty(encryptionKey))
{
    // Encryption key is required - application cannot run without it
    throw new InvalidOperationException("EncryptionKey is missing in appsettings.json");
}
Assignment.Services.EncryptionService.Initialize(encryptionKey);

// ========== AUTHENTICATION CONFIGURATION ==========
// Configure Cookie-based Authentication
// Uses custom cookie authentication (NOT ASP.NET Core Identity)
// Authentication cookies store user claims (ID, email, role, name) for 7 days
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Security/Login";           // Redirect to login page if user is not authenticated
        options.LogoutPath = "/Security/Logout";          // Path for logout action
        options.AccessDeniedPath = "/Home/AccessDenied";  // Redirect to this page if user doesn't have required role
        options.ExpireTimeSpan = TimeSpan.FromDays(7);    // Authentication cookie expires after 7 days
        options.SlidingExpiration = true;                 // Reset expiration timer on each request (keeps user logged in if active)
    });

// ========== MVC SERVICES ==========
// Add MVC services (Controllers and Views)
// This enables the Model-View-Controller pattern for handling HTTP requests
builder.Services.AddControllersWithViews();

// ========== MEMORY CACHE ==========
// Add distributed memory cache for storing temporary data
// Used for session state and caching
builder.Services.AddDistributedMemoryCache();

// ========== SESSION CONFIGURATION ==========
// Configure session state for storing temporary data (e.g., captcha answers, form data)
// Session data is stored server-side and linked to user via session cookie
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);  // Session expires after 30 minutes of inactivity
    options.Cookie.HttpOnly = true;                   // Prevent JavaScript access to session cookie (XSS protection)
    options.Cookie.IsEssential = true;                // Required for GDPR compliance (essential cookies don't need consent)
});

// ========== COOKIE POLICY ==========
// Configure cookie policy settings
// This permission is required for cookies to stay across pages
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;        // Don't require cookie consent (for assignment simplicity)
    options.MinimumSameSitePolicy = SameSiteMode.Lax;    // Allow cookies in cross-site requests (for OAuth, etc.)
});

// ========== CUSTOM SERVICES REGISTRATION ==========
// Register custom services as scoped (one instance per HTTP request)
// These services can be injected into controllers using dependency injection

// Register PromotionValidationService for validating promotion codes and preventing abuse
builder.Services.AddScoped<Assignment.Services.PromotionValidationService>();

// Register EmailService for sending emails (verification, password reset OTP)
builder.Services.AddScoped<Assignment.Services.EmailService>();

// Register BookingStatusUpdateService for automatically updating booking statuses (check-in, check-out, no-show)
builder.Services.AddScoped<Assignment.Services.BookingStatusUpdateService>();

// ========== BUILD APPLICATION ==========
// Build the application instance from the configured services
// This creates the WebApplication object that handles HTTP requests
var app = builder.Build();

// ========== LOCALIZATION CONFIGURATION ==========
// Configure date and number formatting for the application
// Using UK/Malaysia format (dd/MM/yyyy) for dates
// Currency symbol set to RM (Malaysian Ringgit)
var defaultDateCulture = "en-GB"; // UK/Malaysia format (dd/MM/yyyy)
var ci = new System.Globalization.CultureInfo(defaultDateCulture);
ci.NumberFormat.CurrencySymbol = "RM"; // Set Currency Symbol to RM (Malaysian Ringgit)

// Apply localization settings to all requests
// This ensures dates and currency are displayed consistently throughout the application
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(ci),
    SupportedCultures = new List<System.Globalization.CultureInfo> { ci },
    SupportedUICultures = new List<System.Globalization.CultureInfo> { ci }
});

// ========== 2. Database Initialization ==========
// Apply migrations, seed initial data, and perform database maintenance tasks

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<HotelDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        // ========== APPLY DATABASE MIGRATIONS ==========
        // Apply pending Entity Framework migrations to update database schema
        // Migrations add new tables, columns, and modify existing structures
        // This ensures the database schema matches the current model definitions
        try
        {
            context.Database.Migrate();
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Message.Contains("There is already an object named"))
        {
            // ========== HANDLE MANUAL DATABASE CREATION ==========
            // Handle case where database tables exist but migration history is missing
            // This happens when database was created manually or from SQL scripts
            // We need to create the migration history table and mark migrations as applied
            logger.LogWarning("Database tables already exist. Marking migration as applied...");
            
            // ========== CREATE MIGRATION HISTORY TABLE ==========
            // Ensure migration history table exists
            // This table tracks which migrations have been applied
            context.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '__EFMigrationsHistory')
                BEGIN
                    CREATE TABLE [__EFMigrationsHistory] (
                        [MigrationId] nvarchar(150) NOT NULL,
                        [ProductVersion] nvarchar(32) NOT NULL,
                        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                    );
                END
            ");
            
            // ========== MARK INITIAL MIGRATION AS APPLIED ==========
            // Mark initial migration as applied if not already recorded
            // This prevents EF Core from trying to create tables that already exist
            context.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251123141346_IntialCreate')
                BEGIN
                    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                    VALUES ('20251123141346_IntialCreate', '8.0.0');
                END
            ");
            
            logger.LogInformation("Migration marked as applied. Continuing with database initialization...");
        }
        
        // ========== SCHEMA COMPATIBILITY CHECKS ==========
        // Add missing columns to existing tables for backward compatibility
        // These checks ensure the database schema matches the latest model definitions
        // This is useful when database was created from SQL scripts instead of migrations
        
        // ========== ADD IMAGEURL COLUMN TO HOTELS ==========
        // Add ImageUrl column to Hotels table if it doesn't exist (backward compatibility)
        // This column stores the URL/path to the hotel's main image
        try
        {
            context.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Hotels') AND name = 'ImageUrl')
                BEGIN
                    ALTER TABLE Hotels ADD ImageUrl nvarchar(255) NULL;
                END
            ");
        }
        catch (Exception ex)
        {
            logger.LogWarning($"Could not add ImageUrl column: {ex.Message}");
        }
        
        // ========== ADD CATEGORY COLUMN TO HOTELS ==========
        // Add Category column to Hotels table if it doesn't exist
        // Category: 0=Budget, 1=MidRange, 2=Luxury
        // Used to categorize hotels by price range and amenities
        try
        {
            context.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Hotels') AND name = 'Category')
                BEGIN
                    ALTER TABLE Hotels ADD Category int NOT NULL DEFAULT 0;
                END
            ");
        }
        catch (Exception ex)
        {
            logger.LogWarning($"Could not add Category column: {ex.Message}");
        }
        
        // ========== ADD SOURCE COLUMN TO BOOKINGS ==========
        // Add Source column to Bookings table if it doesn't exist
        // Source: 0=Direct, 1=OTA, 2=Group, 3=Phone, 4=WalkIn
        // Used to track where bookings come from (website, travel agency, phone, etc.)
        try
        {
            context.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Bookings') AND name = 'Source')
                BEGIN
                    ALTER TABLE Bookings ADD Source int NOT NULL DEFAULT 0;
                END
            ");
        }
        catch (Exception ex)
        {
            logger.LogWarning($"Could not add Source column: {ex.Message}");
        }
        
        // ========== SEED INITIAL DATA ==========
        // Seed the database with initial data (hotels, users, rooms, etc.)
        // This provides sample data for testing and demonstration
        // Only seeds if data doesn't already exist (prevents duplicates)
        DbInitializer.Initialize(context);
        
        // ========== ADD MISSING ROOM TYPES ==========
        // Add missing room types for specific cities (Ipoh, Kota Kinabalu, Kuching, Cameron Highlands)
        // This ensures all hotels have at least one room type for booking
        Assignment.Models.Data.AddMissingRoomTypes.AddMissingData(context).GetAwaiter().GetResult();
        
        // ========== CLEANUP INVALID PROMOTIONS ==========
        // Clean up invalid promotions on startup (expired, max uses reached, etc.)
        // This ensures only valid promotions are shown to users
        // Automatically deactivates promotions that are no longer valid
        try
        {
            var promotionValidation = services.GetRequiredService<Assignment.Services.PromotionValidationService>();
            var deactivatedCount = promotionValidation.DeactivateInvalidPromotionsAsync().GetAwaiter().GetResult();
            if (deactivatedCount > 0)
            {
                logger.LogInformation($"Deactivated {deactivatedCount} invalid promotion(s) on startup.");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning($"Could not clean up promotions on startup: {ex.Message}");
        }

        // ========== UPDATE BOOKING STATUSES ==========
        // Update booking statuses automatically on startup (check-in, check-out, no-show)
        // This ensures bookings are in the correct status based on current date
        // Example: If check-out date passed, booking status changes from CheckedIn to CheckedOut
        try
        {
            var bookingStatusUpdate = services.GetRequiredService<Assignment.Services.BookingStatusUpdateService>();
            var updatedCount = bookingStatusUpdate.UpdateBookingStatusesAsync().GetAwaiter().GetResult();
            if (updatedCount > 0)
            {
                logger.LogInformation($"Automatically updated {updatedCount} booking status(es) on startup.");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning($"Could not update booking statuses on startup: {ex.Message}");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}


// ========== 3. CONFIGURE HTTP REQUEST PIPELINE ==========
// Middleware is executed in the order it is added here
// Each middleware processes the request and passes it to the next middleware
// Order matters: earlier middleware can modify the request/response before later middleware sees it

// ========== ERROR HANDLING ==========
// Configure error handling based on environment (Development vs Production)
if (!app.Environment.IsDevelopment())
{
    // ========== PRODUCTION ERROR HANDLING ==========
    // Production: Use custom error page (user-friendly, doesn't expose technical details)
    // Enable HSTS (HTTP Strict Transport Security) to force HTTPS connections
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();  // Force HTTPS in production (prevents man-in-the-middle attacks)
}
else
{
    // ========== DEVELOPMENT ERROR HANDLING ==========
    // Development: Show detailed error pages for debugging
    // Includes stack traces, exception details, and source code context
    app.UseDeveloperExceptionPage();
}

// ========== CUSTOM ERROR PAGES ==========
// Custom error pages for HTTP status codes (404 Not Found, 403 Forbidden, 500 Internal Server Error, etc.)
// Re-executes the request to the Error action with the status code
// This provides user-friendly error pages instead of default browser error pages
app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

// ========== SECURITY HEADERS MIDDLEWARE ==========
// Add security headers middleware (X-Frame-Options, CSP, X-XSS-Protection, etc.)
// These headers protect against common web vulnerabilities:
// - Clickjacking (X-Frame-Options)
// - Cross-site scripting (XSS) attacks
// - MIME type sniffing
// - Information leakage
app.UseMiddleware<Assignment.Middleware.SecurityHeadersMiddleware>();

// ========== HTTPS REDIRECTION ==========
// Redirect HTTP requests to HTTPS
// This ensures all traffic is encrypted (important for security, especially login/payment pages)
app.UseHttpsRedirection();

// ========== STATIC FILES ==========
// Serve static files (CSS, JavaScript, images) from wwwroot folder
// Static files are served directly without going through controllers
// Examples: /css/style.css, /js/script.js, /images/logo.png
app.UseStaticFiles();

// ========== COOKIE POLICY ==========
// Apply cookie policy settings configured above
// Enforces cookie consent and SameSite policies
app.UseCookiePolicy();

// ========== SESSION ==========
// Enable session state for storing temporary data
// Session data is stored server-side and linked to user via session cookie
// Used for: captcha answers, form data, temporary state
app.UseSession();

// ========== ROUTING ==========
// Enable routing to match URLs to controllers and actions
// Example: /Booking/Create → BookingController.Create action
// This must come before UseAuthentication and UseAuthorization
app.UseRouting();

// ========== AUTHENTICATION ==========
// Enable authentication (must come before authorization)
// Checks if user is logged in by reading authentication cookie
// Sets HttpContext.User with user claims (ID, email, role, name)
app.UseAuthentication();

// ========== AUTHORIZATION ==========
// Enable authorization (role-based access control)
// Checks if authenticated user has required role/permissions
// Uses [Authorize] and [AuthorizeRole] attributes on controllers/actions
app.UseAuthorization();

// ========== ROUTE CONFIGURATION ==========
// Configure default route: /Controller/Action/Id
// Defaults to Home/Index if no controller/action is specified
// Examples:
// - / → HomeController.Index
// - /Booking/Create → BookingController.Create
// - /Admin/Users/5 → AdminController.Users with id=5
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ========== START APPLICATION ==========
// Start the application and begin listening for HTTP requests
// The application will run until it is stopped (Ctrl+C or shutdown)
app.Run();