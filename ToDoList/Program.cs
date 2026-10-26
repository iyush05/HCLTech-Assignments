if (args.Contains("--self-test")) { SelfTests.Run(); return; }
var builder = WebApplication.CreateBuilder(args);
string dataPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "tasks.json");
builder.Services.AddSingleton<TaskService>(_ => new TaskService(new JsonStore<List<TodoItem>>(dataPath)));
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
app.MapGet("/api/tasks", (string? filter, string? search, TaskService service) => service.List(filter, search));
app.MapPost("/api/tasks", (TaskRequest request, TaskService service) => service.Add(request.Title, request.Priority));
app.MapPut("/api/tasks/{id:guid}", (Guid id, TaskRequest request, TaskService service) => service.Update(id, request.Title, request.Priority, request.Completed));
app.MapDelete("/api/tasks/{id:guid}", (Guid id, TaskService service) => service.Delete(id) ? Results.NoContent() : Results.NotFound(new { error = "Task not found." }));
app.Run();
record TaskRequest(string Title, string Priority, bool Completed);
