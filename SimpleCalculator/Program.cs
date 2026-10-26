using System.Globalization;
using SimpleCalculator;
if (args.Contains("--console")) { ConsoleApp.Run(); return; }
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapPost("/api/calculate", (CalculationRequest request) => {
    if (!decimal.TryParse(request.First, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal first)
        || !decimal.TryParse(request.Second, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal second))
        return Results.BadRequest(new { error = "Enter two valid numbers within the decimal range." });
    if (request.Operation is not ("+" or "-" or "*" or "/"))
        return Results.BadRequest(new { error = "Choose a supported operation." });
    if (request.Operation == "/" && second == 0)
        return Results.BadRequest(new { error = "Cannot divide by zero. Try a different number." });
    try {
        decimal result = request.Operation switch {
            "+" => first + second, "-" => first - second,
            "*" => first * second, _ => first / second
        };
        return Results.Ok(new { result = result.ToString(CultureInfo.InvariantCulture) });
    } catch (OverflowException) {
        return Results.BadRequest(new { error = "That result is too large. Try smaller numbers." });
    }
});
app.Run();
record CalculationRequest(string First, string Second, string Operation);
