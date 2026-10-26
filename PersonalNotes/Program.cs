using Microsoft.EntityFrameworkCore;

if (args.Contains("--self-test")) { SelfTests.Run(); return; }

var builder = WebApplication.CreateBuilder(args);

string dataDir = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataDir);
string dbPath = Path.Combine(dataDir, "notes.db");

builder.Services.AddDbContext<NotesDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddScoped<NoteService>();

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
        await context.Response.WriteAsJsonAsync(new { error = "Invalid request format or missing required payload." });
    }
    catch (Exception error) {
        app.Logger.LogError(error, "PersonalNotes server error occurred.");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "A database or internal server error occurred." });
    }
});

app.UseDefaultFiles();
app.UseStaticFiles();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotesDbContext>();
    db.Database.EnsureCreated();
    if (!db.Notes.Any())
    {
        db.Notes.AddRange(
            new Note
            {
                Title = "Welcome to Personal Notes",
                Content = "This app demonstrates ASP.NET Core Minimal APIs combined with Entity Framework Core and an embedded SQLite database.\n\nKey capabilities:\n- Instant search across titles & contents\n- Categorization and color tagging\n- Pinning critical items to top\n- Word and note telemetry",
                Category = "General",
                Color = "mint",
                IsPinned = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Note
            {
                Title = "C# & EF Core Best Practices",
                Content = "1. Always use AsNoTracking() for read-only queries\n2. Index frequently searched columns\n3. Use DTOs/records for API boundaries\n4. Leverage async/await across all database I/O",
                Category = "Ideas",
                Color = "sky",
                IsPinned = false,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-10)
            }
        );
        db.SaveChanges();
    }
}

app.MapGet("/api/notes", (string? category, string? search, bool? pinned, NoteService service) => service.ListAsync(category, search, pinned));
app.MapGet("/api/notes/{id:guid}", async (Guid id, NoteService service) => await service.GetByIdAsync(id) is { } note ? Results.Ok(note) : Results.NotFound(new { error = "Note not found." }));
app.MapPost("/api/notes", async (NoteRequest request, NoteService service) => Results.Created($"/api/notes", await service.AddAsync(request)));
app.MapPut("/api/notes/{id:guid}", async (Guid id, NoteRequest request, NoteService service) => Results.Ok(await service.UpdateAsync(id, request)));
app.MapDelete("/api/notes/{id:guid}", async (Guid id, NoteService service) => await service.DeleteAsync(id) ? Results.NoContent() : Results.NotFound(new { error = "Note not found." }));
app.MapPatch("/api/notes/{id:guid}/pin", async (Guid id, NoteService service) => Results.Ok(await service.TogglePinAsync(id)));
app.MapGet("/api/notes/stats", (NoteService service) => service.GetStatsAsync());
app.MapGet("/api/notes/categories", (NoteService service) => service.GetCategoriesAsync());

app.Run();
