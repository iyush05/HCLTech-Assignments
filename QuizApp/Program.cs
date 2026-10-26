if (args.Contains("--self-test")) { SelfTests.Run(); return; }
var builder = WebApplication.CreateBuilder(args);
string dataPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "attempts.json");
builder.Services.AddSingleton<QuizService>(_ => new QuizService(Path.Combine(builder.Environment.ContentRootPath, "Questions.json"), new JsonStore<List<QuizAttempt>>(dataPath)));
var app = builder.Build();
app.Use(async (context, next) =>
{
    try { await next(context); }
    catch (ValidationException error) {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { error = error.Message });
    }
    catch (BadHttpRequestException) {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { error = "The request contains invalid or missing values." });
    }
    catch (Exception error) when (error is IOException or System.Text.Json.JsonException or UnauthorizedAccessException) {
        app.Logger.LogError(error, "Unable to access the data file.");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "Unable to read or save the data file. Check the server terminal and file permissions." });
    }
});
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapGet("/api/questions", (QuizService service) => service.Questions());
app.MapGet("/api/history", (QuizService service) => service.History());
app.MapPost("/api/submit", (QuizSubmission request, QuizService service) => service.Submit(request.Answers));
app.Run();
record QuizSubmission(Dictionary<int, int>? Answers);
