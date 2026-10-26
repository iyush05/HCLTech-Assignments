# Weather App

An independent ASP.NET Core 8 Web application demonstrating typed `HttpClient` integration, external REST weather services, fallback resilience, and Entity Framework Core 8 with SQLite.
Includes its own responsive browser interface in `wwwroot/`.

## Run

Install the .NET 8 SDK, or run `bash setup.sh` from the repository root. Then:

```sh
# From the repository root:
source ./dotnet-env.sh
cd WeatherApp
dotnet run
```

Open **http://localhost:5110**. Press Ctrl+C to stop.
Run `bash run-all.sh` from the repository root to start all projects simultaneously (ports 5101–5111).

## Features

- **Live Meteorological Data**: Fetches real-time temperatures, conditions, humidity, wind velocity, and precipitation via Open-Meteo REST API.
- **24-Hour & 7-Day Forecasts**: Visual hourly timeline and 7-day extended forecasts with dynamic weather icons.
- **Global City Search & Autocomplete**: Search any world location with debounced live suggestions.
- **Unit Conversion**: Instant toggle between Celsius (°C) and Fahrenheit (°F) with automatic speed/precipitation conversions.
- **Saved Favorites (EF Core)**: Persist favorite locations in local SQLite database (`App_Data/weather.db`).
- **Offline Resilience**: Automatic fallback simulation ensures the application operates gracefully even without internet access or when tests run offline.

## Web, HTTP & EF Core concepts

| Topic | Implementation |
| --- | --- |
| `HttpClient` & Factory | `builder.Services.AddHttpClient<WeatherService>()` with custom timeouts and headers. |
| External REST APIs | Consumes Open-Meteo geocoding and forecasting endpoints. |
| Resilient Fallbacks | Handles network disconnects and rate limits with deterministic offline simulation. |
| JSON Deserialization | `System.Text.Json` parses nested JSON payloads without third-party libraries. |
| EF Core SQLite | Stores favorite cities and search history with unique indexes and timestamps. |
| Self-Tests | `SelfTests.cs` validates weather code mapping, unit calculations, and EF Core operations in-memory. |

## Files

- `Program.cs`: ASP.NET Core host, `HttpClient` configuration, EF Core SQLite setup, and Minimal API routes.
- `WeatherModels.cs`: DTO models, WMO code helper, and request records.
- `WeatherService.cs`: Business logic, HTTP API consumption, fallback handling, and EF Core database access.
- `WeatherDbContext.cs`: EF Core `DbContext` managing `Favorites` and `Searches` tables.
- `SelfTests.cs`: Executable tests covering unit conversions, weather code translations, and database operations.
- `wwwroot/index.html`, `styles.css`, `app.js`: Interactive frontend matching the Form & Function design system.
- `Properties/launchSettings.json`: Port 5110 configuration.

## Build and self-tests

```sh
dotnet build
dotnet run -- --self-test
```
