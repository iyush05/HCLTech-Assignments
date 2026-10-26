if (args.Contains("--self-test")) { SelfTests.Run(); return; }
var builder = WebApplication.CreateBuilder(args);
string dataPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "students.json");
builder.Services.AddSingleton<GradeService>(_ => new GradeService(new JsonStore<List<Student>>(dataPath)));
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
app.MapGet("/api/students", (string? search, string? sort, GradeService service) => service.List(search, sort));
app.MapPost("/api/students", (StudentRequest request, GradeService service) => service.Save(null, request));
app.MapPut("/api/students/{id:guid}", (Guid id, StudentRequest request, GradeService service) => service.Save(id, request));
app.MapDelete("/api/students/{id:guid}", (Guid id, GradeService service) => service.Delete(id) ? Results.NoContent() : Results.NotFound(new { error = "Student not found." }));
app.Run();
