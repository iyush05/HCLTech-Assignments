using System.Security.Cryptography;
using NumberGuessingGame;
if (args.Contains("--console")) { ConsoleApp.Run(); return; }
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.Cookie.Name = "NumberLab.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.IdleTimeout = TimeSpan.FromHours(2);
});
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSession();
app.MapPost("/api/game", (HttpContext context) => {
    context.Session.Clear();
    context.Session.SetInt32("secret", RandomNumberGenerator.GetInt32(1, 101));
    context.Session.SetInt32("attempts", 0);
    return Results.Ok(new { message = "A fresh number is waiting. Make your first guess." });
});
app.MapPost("/api/guess", (GuessRequest request, HttpContext context) => {
    int? secret = context.Session.GetInt32("secret");
    if (secret is null) return Results.BadRequest(new { error = "Your round expired. Start a new round." });
    if (context.Session.GetInt32("won") == 1)
        return Results.BadRequest(new { error = "You solved this round! Start a new one." });
    if (request.Guess < 1 || request.Guess > 100)
        return Results.BadRequest(new { error = "Choose a whole number between 1 and 100." });
    int attempts = (context.Session.GetInt32("attempts") ?? 0) + 1;
    context.Session.SetInt32("attempts", attempts);
    string direction = request.Guess == secret ? "correct" : request.Guess < secret ? "higher" : "lower";
    if (direction == "correct") context.Session.SetInt32("won", 1);
    return Results.Ok(new { direction, attempts });
});
app.Run();
record GuessRequest(int Guess);
