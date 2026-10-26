using Microsoft.EntityFrameworkCore;

if (args.Contains("--self-test")) { SelfTests.Run(); return; }

var builder = WebApplication.CreateBuilder(args);

string dataDir = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataDir);
string dbPath = Path.Combine(dataDir, "weather.db");

builder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddHttpClient<WeatherService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(6);
    client.DefaultRequestHeaders.Add("User-Agent", "FormAndFunction-WeatherApp/1.0");
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    try { await next(context); }
    catch (WeatherValidationException error) {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { error = error.Message });
    }
    catch (BadHttpRequestException) {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { error = "Invalid request format." });
    }
    catch (Exception error) {
        app.Logger.LogError(error, "WeatherApp error occurred.");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "An internal server error occurred while retrieving weather data." });
    }
});

app.UseDefaultFiles();
app.UseStaticFiles();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
    db.Database.EnsureCreated();
    if (!db.Favorites.Any())
    {
        db.Favorites.AddRange(
            new FavoriteCity { City = "Tokyo", Country = "Japan", Latitude = 35.6762, Longitude = 139.6503, AddedAt = DateTime.UtcNow },
            new FavoriteCity { City = "London", Country = "United Kingdom", Latitude = 51.5074, Longitude = -0.1278, AddedAt = DateTime.UtcNow.AddMinutes(-5) },
            new FavoriteCity { City = "New York", Country = "United States", Latitude = 40.7128, Longitude = -74.0060, AddedAt = DateTime.UtcNow.AddMinutes(-10) }
        );
        db.SaveChanges();
    }
}

app.MapGet("/api/weather/search", (string? query, WeatherService svc) => svc.SearchLocationsAsync(query));
app.MapGet("/api/weather/report", (double lat, double lon, string? city, string? country, string? unit, WeatherService svc) => svc.GetReportAsync(lat, lon, city, country, unit ?? "c"));
app.MapGet("/api/weather/favorites", (WeatherService svc) => svc.GetFavoritesAsync());
app.MapPost("/api/weather/favorites", async (AddFavoriteRequest req, WeatherService svc) => Results.Created("/api/weather/favorites", await svc.AddFavoriteAsync(req)));
app.MapDelete("/api/weather/favorites/{id:guid}", async (Guid id, WeatherService svc) => await svc.RemoveFavoriteAsync(id) ? Results.NoContent() : Results.NotFound(new { error = "Favorite city not found." }));
app.MapGet("/api/weather/history", (WeatherService svc) => svc.GetRecentSearchesAsync());

app.Run();
