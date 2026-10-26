public record GeoLocation(
    string Name,
    string Country,
    double Latitude,
    double Longitude,
    string? Admin1
);

public record CurrentWeather(
    double Temperature,
    double FeelsLike,
    int Humidity,
    double WindSpeed,
    double Precipitation,
    int WeatherCode,
    string WeatherDescription,
    string WeatherIcon,
    bool IsDay
);

public record HourlyForecastItem(
    string Time,
    double Temperature,
    int WeatherCode,
    string WeatherIcon
);

public record DailyForecastItem(
    string Date,
    string DayName,
    double MaxTemp,
    double MinTemp,
    double PrecipitationSum,
    int WeatherCode,
    string WeatherDescription,
    string WeatherIcon
);

public record WeatherReport(
    string City,
    string Country,
    double Latitude,
    double Longitude,
    string Unit,
    CurrentWeather Current,
    List<HourlyForecastItem> Hourly,
    List<DailyForecastItem> Daily,
    bool IsOfflineData
);

public record AddFavoriteRequest(
    string? City,
    string? Country,
    double Latitude,
    double Longitude
);

public static class WeatherCodeHelper
{
    public static (string Description, string Icon) GetDetails(int code, bool isDay = true)
    {
        return code switch
        {
            0 => (isDay ? "Clear sky" : "Clear night", isDay ? "☀️" : "🌙"),
            1 => ("Mainly clear", isDay ? "🌤️" : "🌤️"),
            2 => ("Partly cloudy", "⛅"),
            3 => ("Overcast", "☁️"),
            45 or 48 => ("Foggy", "🌫️"),
            51 or 53 or 55 => ("Drizzle", "🌦️"),
            56 or 57 => ("Freezing drizzle", "🌨️"),
            61 => ("Slight rain", "🌧️"),
            63 => ("Moderate rain", "🌧️"),
            65 => ("Heavy rain", "🌧️"),
            66 or 67 => ("Freezing rain", "🌨️"),
            71 or 73 or 75 => ("Snow fall", "❄️"),
            77 => ("Snow grains", "❄️"),
            80 or 81 or 82 => ("Rain showers", "🌦️"),
            85 or 86 => ("Snow showers", "🌨️"),
            95 => ("Thunderstorm", "⛈️"),
            96 or 99 => ("Thunderstorm with hail", "⛈️"),
            _ => ("Fair", isDay ? "☀️" : "🌙")
        };
    }
}
