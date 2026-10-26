using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

public static class SelfTests
{
    public static void Run()
    {
        Console.WriteLine("Running WeatherApp Self-Tests (HTTP Models & EF Core)...");

        // 1. WeatherCodeHelper
        var (desc0, icon0) = WeatherCodeHelper.GetDetails(0, true);
        Assert(desc0.Contains("Clear"), "Code 0 should be Clear sky");
        var (desc95, icon95) = WeatherCodeHelper.GetDetails(95, true);
        Assert(desc95.Contains("Thunderstorm"), "Code 95 should be Thunderstorm");

        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<WeatherDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var db = new WeatherDbContext(options))
        {
            db.Database.EnsureCreated();

            using var httpClient = new HttpClient();
            var service = new WeatherService(httpClient, db, NullLogger<WeatherService>.Instance);

            // 2. Search fallback
            var searchResults = service.SearchLocationsAsync("Tokyo").GetAwaiter().GetResult();
            Assert(searchResults.Count > 0, "Search should return results for Tokyo");
            Assert(searchResults.Any(c => c.Name == "Tokyo"), "Tokyo should be in search results");

            // 3. Fallback / offline report generation
            var report = service.GetReportAsync(35.6762, 139.6503, "Tokyo", "Japan", "c").GetAwaiter().GetResult();
            Assert(report.City == "Tokyo", "City should match Tokyo");
            Assert(report.Hourly.Count >= 24, "Hourly forecast should have at least 24 hours");
            Assert(report.Daily.Count >= 7, "Daily forecast should have at least 7 days");

            // 4. Unit Conversion
            var reportF = service.GetReportAsync(35.6762, 139.6503, "Tokyo", "Japan", "f").GetAwaiter().GetResult();
            Assert(reportF.Unit == "°F", "Unit should be °F");

            // 5. Add Favorite via EF Core
            var fav = service.AddFavoriteAsync(new AddFavoriteRequest("Tokyo", "Japan", 35.6762, 139.6503)).GetAwaiter().GetResult();
            Assert(fav.Id != Guid.Empty, "Favorite ID should be generated");

            // 6. List Favorites
            var favorites = service.GetFavoritesAsync().GetAwaiter().GetResult();
            Assert(favorites.Count == 1, $"Expected 1 favorite, got {favorites.Count}");
            Assert(favorites[0].City == "Tokyo", "Favorite city should be Tokyo");

            // 7. Duplicate favorite idempotency
            var fav2 = service.AddFavoriteAsync(new AddFavoriteRequest("Tokyo", "Japan", 35.6762, 139.6503)).GetAwaiter().GetResult();
            Assert(fav2.Id == fav.Id, "Duplicate favorite should return existing entity");
            var favoritesAfterDup = service.GetFavoritesAsync().GetAwaiter().GetResult();
            Assert(favoritesAfterDup.Count == 1, "Duplicate favorite should not increase count");

            // 8. Add second favorite and check ordering
            service.AddFavoriteAsync(new AddFavoriteRequest("London", "United Kingdom", 51.5074, -0.1278)).GetAwaiter().GetResult();
            var favoritesTwo = service.GetFavoritesAsync().GetAwaiter().GetResult();
            Assert(favoritesTwo.Count == 2, "Should have 2 favorites");
            Assert(favoritesTwo[0].City == "London", "Most recently added should be first");

            // 9. Validation Exception
            bool caughtValidation = false;
            try
            {
                service.AddFavoriteAsync(new AddFavoriteRequest("", "Nowhere", 0, 0)).GetAwaiter().GetResult();
            }
            catch (WeatherValidationException)
            {
                caughtValidation = true;
            }
            Assert(caughtValidation, "Empty city should throw WeatherValidationException");

            // 10. Delete Favorite
            var removed = service.RemoveFavoriteAsync(fav.Id).GetAwaiter().GetResult();
            Assert(removed, "RemoveFavorite should return true");
            var favoritesRemaining = service.GetFavoritesAsync().GetAwaiter().GetResult();
            Assert(favoritesRemaining.Count == 1, "Should have 1 favorite remaining");
        }

        Console.WriteLine("All 10 WeatherApp checks passed successfully.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new Exception($"Test assertion failed: {message}");
    }
}
