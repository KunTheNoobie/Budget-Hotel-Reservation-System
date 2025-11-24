using Assignment.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<HotelDbContext>(options =>
    options.UseSqlServer(connectionString));

// Initialize Encryption Service
var encryptionKey = builder.Configuration["EncryptionKey"];
if (string.IsNullOrEmpty(encryptionKey))
{
    throw new InvalidOperationException("EncryptionKey is missing in appsettings.json");
}
Assignment.Services.EncryptionService.Initialize(encryptionKey);

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Security/Login";
        options.LogoutPath = "/Security/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register PromotionValidationService
builder.Services.AddScoped<Assignment.Services.PromotionValidationService>();

var app = builder.Build();

// 2. Apply migrations and seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<HotelDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        // Apply pending migrations
        try
        {
            context.Database.Migrate();
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx) when (sqlEx.Message.Contains("There is already an object named"))
        {
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
            
            // Mark migration as applied if not already recorded
            context.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251123141346_IntialCreate')
                BEGIN
                    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                    VALUES ('20251123141346_IntialCreate', '8.0.0');
                END
            ");
            
            logger.LogInformation("Migration marked as applied. Continuing with database initialization...");
        }
        
        // Add ImageUrl column to Hotels table if it doesn't exist
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
        
        DbInitializer.Initialize(context);
        
        // Add missing room types for specific cities
        Assignment.Models.Data.AddMissingRoomTypes.AddMissingData(context).GetAwaiter().GetResult();
        
        // Clean up invalid promotions on startup
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
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}


// 3. Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseMiddleware<Assignment.Middleware.SecurityHeadersMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();