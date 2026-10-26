using System.Globalization;
using UnitConverter;
if (args.Contains("--console")) { ConsoleApp.Run(); return; }
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapPost("/api/convert", (ConversionRequest request) => {
    if (request.Option < 1 || request.Option > 6)
        return Results.BadRequest(new { error = "Choose a supported conversion." });
    if (!double.TryParse(request.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double value)
        || !double.IsFinite(value))
        return Results.BadRequest(new { error = "Enter a finite number to convert." });
    if (request.Option <= 4 && value < 0)
        return Results.BadRequest(new { error = "Distance and weight cannot be negative." });
    if ((request.Option == 5 && value < -273.15) || (request.Option == 6 && value < -459.67))
        return Results.BadRequest(new { error = "Temperature cannot be below absolute zero." });
    double result = request.Option switch {
        1 => value / 1.609344, 2 => value * 1.609344,
        3 => value / 0.45359237, 4 => value * 0.45359237,
        5 => value * 1.8 + 32, _ => (value - 32) / 1.8
    };
    return double.IsFinite(result)
        ? Results.Ok(new { result = result.ToString("G12", CultureInfo.InvariantCulture) })
        : Results.BadRequest(new { error = "The result is too large. Try a smaller number." });
});
app.Run();
record ConversionRequest(string Value, int Option);
