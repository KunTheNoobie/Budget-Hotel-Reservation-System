using Assignment.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

// ========== Application Entry Point ==========
// This is the main entry point for the ASP.NET Core application.
// Configures services, database, authentication, and the HTTP request pipeline.

var builder = WebApplication.CreateBuilder(args);

// ========== 1. Configure Services ==========

// Configure database connection using Entity Framework Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

// Initialize Encryption Service for encrypting sensitive data (e.g., phone numbers)
var encryptionKey = builder.Configuration["EncryptionKey"];
if (string.IsNullOrEmpty(encryptionKey))
{
    throw new InvalidOperationException("EncryptionKey is missing in appsettings.json");
}
Assignment.Services.EncryptionService.Initialize(encryptionKey);

// Configure Cookie-based Authentication
// Uses custom cookie authentication (not ASP.NET Core Identity)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Security/Login";           // Redirect to login if not authenticated
        options.LogoutPath = "/Security/Logout";          // Path for logout
        options.AccessDeniedPath = "/Home/AccessDenied";  // Redirect if access denied
        options.ExpireTimeSpan = TimeSpan.FromDays(7);    // Cookie expires after 7 days
        options.SlidingExpiration = true;                 // Reset expiration on each request
    });

// Add MVC services (Controllers and Views)
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();

// Configure session state for storing temporary data
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);  // Session expires after 30 minutes of inactivity
    options.Cookie.HttpOnly = true;                   // Prevent JavaScript access to session cookie
    options.Cookie.IsEssential = true;                // Required for GDPR compliance
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This permission is required for cookies to stay across pages
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});

// Register PromotionValidationService as a scoped service (one instance per HTTP request)
builder.Services.AddScoped<Assignment.Services.PromotionValidationService>();

// Register EmailService as a scoped service for sending emails
builder.Services.AddScoped<Assignment.Services.EmailService>();

// Register BookingStatusUpdateService as a scoped service for automatic booking status updates
builder.Services.AddScoped<Assignment.Services.BookingStatusUpdateService>();

// Build the application
var app = builder.Build();

var defaultDateCulture = "en-GB"; // Using UK/Malaysia format (dd/MM/yyyy)
var ci = new System.Globalization.CultureInfo(defaultDateCulture);
ci.NumberFormat.CurrencySymbol = "RM"; // Set Currency Symbol to RM


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
        
        // Apply pending Entity Framework migrations to update database schema
        try
        {
            context.Database.Migrate();
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Message.Contains("There is already an object named"))
        {
            // Handle case where database tables exist but migration history is missing
            // (e.g., database was created manually or from SQL scripts)
            logger.LogWarning("Database tables already exist. Marking migration as applied...");
            
            // Ensure migration history table exists
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
            
            // Mark initial migration as applied if not already recorded
            context.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251123141346_IntialCreate')
                BEGIN
                    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                    VALUES ('20251123141346_IntialCreate', '8.0.0');
                END
            ");
            
            logger.LogInformation("Migration marked as applied. Continuing with database initialization...");
        }
        
        // Add ImageUrl column to Hotels table if it doesn't exist (backward compatibility)
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
        
        // Seed the database with initial data (hotels, users, rooms, etc.)
        DbInitializer.Initialize(context);
        
        // Add missing room types for specific cities (Ipoh, Kota Kinabalu, Kuching, Cameron Highlands)
        Assignment.Models.Data.AddMissingRoomTypes.AddMissingData(context).GetAwaiter().GetResult();
        
        // Clean up invalid promotions on startup (expired, max uses reached, etc.)
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

        // Update booking statuses automatically on startup (check-in, check-out, no-show)
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


// ========== 3. Configure HTTP Request Pipeline ==========
// Middleware is executed in the order it is added here

// Configure error handling based on environment
if (!app.Environment.IsDevelopment())
{
    // Production: Use custom error page and enable HSTS (HTTP Strict Transport Security)
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();  // Force HTTPS in production
}
else
{
    // Development: Show detailed error pages for debugging
    app.UseDeveloperExceptionPage();
}

// Custom error pages for HTTP status codes (404, 403, 500, etc.)
app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

// Add security headers middleware (X-Frame-Options, CSP, etc.)
app.UseMiddleware<Assignment.Middleware.SecurityHeadersMiddleware>();

// Redirect HTTP requests to HTTPS
app.UseHttpsRedirection();

// Serve static files (CSS, JavaScript, images) from wwwroot folder
app.UseStaticFiles();

app.UseCookiePolicy();

app.UseSession();

// Enable routing to match URLs to controllers and actions
app.UseRouting();

// Enable authentication (must come before authorization)
app.UseAuthentication();

// Enable authorization (role-based access control)
app.UseAuthorization();

// Enable session state

// Configure default route: /Controller/Action/Id
// Defaults to Home/Index if no controller/action is specified
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Start the application and begin listening for HTTP requests
app.Run();