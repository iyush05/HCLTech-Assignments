using System.Text.Json;
using Microsoft.EntityFrameworkCore;

public class WeatherValidationException(string message) : Exception(message);

public sealed class WeatherService(HttpClient http, WeatherDbContext db, ILogger<WeatherService> logger)
{
    private static readonly List<GeoLocation> FallbackCities = new()
    {
        new("London", "United Kingdom", 51.5074, -0.1278, "England"),
        new("New York", "United States", 40.7128, -74.0060, "New York"),
        new("Tokyo", "Japan", 35.6762, 139.6503, "Tokyo"),
        new("Paris", "France", 48.8566, 2.3522, "Île-de-France"),
        new("Sydney", "Australia", -33.8688, 151.2093, "New South Wales"),
        new("Mumbai", "India", 19.0760, 72.8777, "Maharashtra"),
        new("San Francisco", "United States", 37.7749, -122.4194, "California"),
        new("Berlin", "Germany", 52.5200, 13.4050, "Berlin"),
        new("Singapore", "Singapore", 1.3521, 103.8198, "Central"),
        new("Toronto", "Canada", 43.6532, -79.3832, "Ontario"),
        new("Dubai", "United Arab Emirates", 25.2048, 55.2708, "Dubai")
    };

    public async Task<List<GeoLocation>> SearchLocationsAsync(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return FallbackCities.Take(5).ToList();

        var q = query.Trim();

        // Record in search history
        try
        {
            db.Searches.Add(new RecentSearch { Query = q, SearchedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to log recent search.");
        }

        try
        {
            var url = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(q)}&count=5&language=en&format=json";
            using var response = await http.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                if (doc.RootElement.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array)
                {
                    var list = new List<GeoLocation>();
                    foreach (var item in results.EnumerateArray())
                    {
                        var name = item.GetProperty("name").GetString() ?? q;
                        var country = item.TryGetProperty("country", out var c) ? c.GetString() ?? "" : "";
                        var lat = item.GetProperty("latitude").GetDouble();
                        var lon = item.GetProperty("longitude").GetDouble();
                        var admin = item.TryGetProperty("admin1", out var a) ? a.GetString() : null;
                        list.Add(new GeoLocation(name, country, lat, lon, admin));
                    }
                    if (list.Count > 0) return list;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogInformation("External geocoding HTTP call failed: {Message}. Using local fallback directory.", ex.Message);
        }

        // Offline / fallback filter
        var filtered = FallbackCities
            .Where(c => c.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                     || c.Country.Contains(q, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (filtered.Count == 0)
        {
            // If unknown city while offline, generate a mock coordinate entry so user can still test
            filtered.Add(new GeoLocation(q, "Global Location", 45.0, 10.0, null));
        }

        return filtered;
    }

    public async Task<WeatherReport> GetReportAsync(double lat, double lon, string? city, string? country, string unit = "c")
    {
        var isFahrenheit = unit.Equals("f", StringComparison.OrdinalIgnoreCase);
        var cityName = string.IsNullOrWhiteSpace(city) ? "Current Location" : city.Trim();
        var countryName = country ?? "";

        try
        {
            var url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current=temperature_2m,relative_humidity_2m,apparent_temperature,is_day,precipitation,weather_code,wind_speed_10m&hourly=temperature_2m,weather_code&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_sum&timezone=auto";
            using var response = await http.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                var root = doc.RootElement;

                var cur = root.GetProperty("current");
                var rawTemp = cur.GetProperty("temperature_2m").GetDouble();
                var rawFeels = cur.GetProperty("apparent_temperature").GetDouble();
                var humidity = cur.GetProperty("relative_humidity_2m").GetInt32();
                var rawWind = cur.GetProperty("wind_speed_10m").GetDouble();
                var rawPrecip = cur.GetProperty("precipitation").GetDouble();
                var code = cur.GetProperty("weather_code").GetInt32();
                var isDay = cur.GetProperty("is_day").GetInt32() == 1;

                var (desc, icon) = WeatherCodeHelper.GetDetails(code, isDay);

                var temp = isFahrenheit ? ToFahrenheit(rawTemp) : Math.Round(rawTemp, 1);
                var feels = isFahrenheit ? ToFahrenheit(rawFeels) : Math.Round(rawFeels, 1);
                var wind = isFahrenheit ? Math.Round(rawWind * 0.621371, 1) : Math.Round(rawWind, 1);

                var currentObj = new CurrentWeather(temp, feels, humidity, wind, rawPrecip, code, desc, icon, isDay);

                // Hourly (next 24 hours)
                var hourlyItems = new List<HourlyForecastItem>();
                if (root.TryGetProperty("hourly", out var hourly))
                {
                    var times = hourly.GetProperty("time").EnumerateArray().ToList();
                    var temps = hourly.GetProperty("temperature_2m").EnumerateArray().ToList();
                    var codes = hourly.GetProperty("weather_code").EnumerateArray().ToList();

                    var now = DateTime.UtcNow;
                    var takeCount = Math.Min(24, times.Count);
                    for (int i = 0; i < takeCount; i++)
                    {
                        var tStr = times[i].GetString() ?? "";
                        var hTime = DateTime.TryParse(tStr, out var parsed) ? parsed.ToString("h tt") : $"{i}:00";
                        var hTempRaw = temps[i].GetDouble();
                        var hTemp = isFahrenheit ? ToFahrenheit(hTempRaw) : Math.Round(hTempRaw, 1);
                        var hCode = codes[i].GetInt32();
                        var (_, hIcon) = WeatherCodeHelper.GetDetails(hCode, true);
                        hourlyItems.Add(new HourlyForecastItem(hTime, hTemp, hCode, hIcon));
                    }
                }

                // Daily (7 days)
                var dailyItems = new List<DailyForecastItem>();
                if (root.TryGetProperty("daily", out var daily))
                {
                    var times = daily.GetProperty("time").EnumerateArray().ToList();
                    var maxes = daily.GetProperty("temperature_2m_max").EnumerateArray().ToList();
                    var mins = daily.GetProperty("temperature_2m_min").EnumerateArray().ToList();
                    var codes = daily.GetProperty("weather_code").EnumerateArray().ToList();
                    var precips = daily.GetProperty("precipitation_sum").EnumerateArray().ToList();

                    for (int i = 0; i < Math.Min(7, times.Count); i++)
                    {
                        var dStr = times[i].GetString() ?? "";
                        var dateObj = DateTime.TryParse(dStr, out var dParsed) ? dParsed : DateTime.UtcNow.AddDays(i);
                        var dayName = i == 0 ? "Today" : dateObj.ToString("ddd");
                        var maxRaw = maxes[i].GetDouble();
                        var minRaw = mins[i].GetDouble();
                        var max = isFahrenheit ? ToFahrenheit(maxRaw) : Math.Round(maxRaw, 1);
                        var min = isFahrenheit ? ToFahrenheit(minRaw) : Math.Round(minRaw, 1);
                        var dCode = codes[i].GetInt32();
                        var precipSum = precips[i].GetDouble();
                        var (dDesc, dIcon) = WeatherCodeHelper.GetDetails(dCode, true);

                        dailyItems.Add(new DailyForecastItem(dateObj.ToString("yyyy-MM-dd"), dayName, max, min, precipSum, dCode, dDesc, dIcon));
                    }
                }

                return new WeatherReport(cityName, countryName, lat, lon, isFahrenheit ? "°F" : "°C", currentObj, hourlyItems, dailyItems, false);
            }
        }
        catch (Exception ex)
        {
            logger.LogInformation("External forecast HTTP call failed: {Message}. Returning offline generated report.", ex.Message);
        }

        return GenerateOfflineReport(lat, lon, cityName, countryName, isFahrenheit);
    }

    public async Task<List<FavoriteCity>> GetFavoritesAsync()
    {
        return await db.Favorites.AsNoTracking().OrderByDescending(f => f.AddedAt).ToListAsync();
    }

    public async Task<FavoriteCity> AddFavoriteAsync(AddFavoriteRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.City))
            throw new WeatherValidationException("City name is required.");

        var city = req.City.Trim();
        var country = req.Country?.Trim() ?? "";

        var existing = await db.Favorites.FirstOrDefaultAsync(f => f.City == city && f.Country == country);
        if (existing is not null) return existing;

        var fav = new FavoriteCity
        {
            City = city,
            Country = country,
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            AddedAt = DateTime.UtcNow
        };

        db.Favorites.Add(fav);
        await db.SaveChangesAsync();
        return fav;
    }

    public async Task<bool> RemoveFavoriteAsync(Guid id)
    {
        var fav = await db.Favorites.FindAsync(id);
        if (fav is null) return false;

        db.Favorites.Remove(fav);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<string>> GetRecentSearchesAsync(int limit = 5)
    {
        return await db.Searches.AsNoTracking()
            .OrderByDescending(s => s.SearchedAt)
            .Select(s => s.Query)
            .Distinct()
            .Take(limit)
            .ToListAsync();
    }

    private static double ToFahrenheit(double celsius) => Math.Round((celsius * 9.0 / 5.0) + 32.0, 1);

    private static WeatherReport GenerateOfflineReport(double lat, double lon, string city, string country, bool isFahrenheit)
    {
        // Deterministic simulation based on latitude
        double baseTemp = Math.Max(5.0, 28.0 - (Math.Abs(lat) * 0.35));
        var code = (Math.Abs((int)lat) + DateTime.UtcNow.Day) % 3 == 0 ? 2 : 0;
        var (desc, icon) = WeatherCodeHelper.GetDetails(code, true);

        var temp = isFahrenheit ? ToFahrenheit(baseTemp) : Math.Round(baseTemp, 1);
        var feels = isFahrenheit ? ToFahrenheit(baseTemp - 1) : Math.Round(baseTemp - 1, 1);
        var cur = new CurrentWeather(temp, feels, 65, isFahrenheit ? 9.5 : 15.0, 0.0, code, desc, icon, true);

        var hourly = new List<HourlyForecastItem>();
        for (int i = 0; i < 24; i++)
        {
            var hTime = DateTime.UtcNow.AddHours(i).ToString("h tt");
            var variation = Math.Sin(i * 0.26) * 3.0;
            var hTempRaw = baseTemp + variation;
            var hTemp = isFahrenheit ? ToFahrenheit(hTempRaw) : Math.Round(hTempRaw, 1);
            hourly.Add(new HourlyForecastItem(hTime, hTemp, code, icon));
        }

        var daily = new List<DailyForecastItem>();
        for (int i = 0; i < 7; i++)
        {
            var d = DateTime.UtcNow.AddDays(i);
            var maxRaw = baseTemp + 2.5 + (i % 2);
            var minRaw = baseTemp - 3.5 - (i % 2);
            var max = isFahrenheit ? ToFahrenheit(maxRaw) : Math.Round(maxRaw, 1);
            var min = isFahrenheit ? ToFahrenheit(minRaw) : Math.Round(minRaw, 1);
            daily.Add(new DailyForecastItem(d.ToString("yyyy-MM-dd"), i == 0 ? "Today" : d.ToString("ddd"), max, min, 0.0, code, desc, icon));
        }

        return new WeatherReport(city, country, lat, lon, isFahrenheit ? "°F" : "°C", cur, hourly, daily, true);
    }
}
