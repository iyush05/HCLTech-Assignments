using Microsoft.EntityFrameworkCore;

if (args.Contains("--self-test")) { SelfTests.Run(); return; }

var builder = WebApplication.CreateBuilder(args);

string dataDir = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dataDir);
string dbPath = Path.Combine(dataDir, "contacts.db");

builder.Services.AddDbContext<ContactDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddScoped<ContactService>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    try { await next(context); }
    catch (ContactValidationException error) {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { error = error.Message });
    }
    catch (BadHttpRequestException) {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { error = "Invalid request format or JSON body." });
    }
    catch (Exception error) {
        app.Logger.LogError(error, "ContactBook internal error occurred.");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "A database or server error occurred." });
    }
});

app.UseDefaultFiles();
app.UseStaticFiles();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ContactDbContext>();
    db.Database.EnsureCreated();
    if (!db.Contacts.Any())
    {
        db.Contacts.AddRange(
            new Contact
            {
                FirstName = "Ada",
                LastName = "Lovelace",
                Email = "ada@computing.org",
                PhoneNumber = "+44 20 7946 0910",
                Company = "Analytical Engine Labs",
                JobTitle = "First Programmer",
                Category = "Work",
                IsFavorite = true,
                Notes = "Author of the first published computer algorithm.",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Contact
            {
                FirstName = "Alan",
                LastName = "Turing",
                Email = "alan@bletchley.org",
                PhoneNumber = "+44 1908 640404",
                Company = "Bletchley Park",
                JobTitle = "Cryptanalyst & Mathematician",
                Category = "Work",
                IsFavorite = true,
                Notes = "Father of theoretical computer science and AI.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-5)
            },
            new Contact
            {
                FirstName = "Grace",
                LastName = "Hopper",
                Email = "grace@navy.mil",
                PhoneNumber = "+1 202 555 0184",
                Company = "US Naval Reserve",
                JobTitle = "Rear Admiral & Computer Scientist",
                Category = "Work",
                IsFavorite = false,
                Notes = "Pioneer of English-like computer programming languages and compilers.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-10)
            },
            new Contact
            {
                FirstName = "Margaret",
                LastName = "Hamilton",
                Email = "margaret@mit.edu",
                PhoneNumber = "+1 617 555 0192",
                Company = "MIT Instrumentation Lab",
                JobTitle = "Director of Software Engineering",
                Category = "Personal",
                IsFavorite = true,
                Notes = "Led the Apollo on-board flight software team.",
                CreatedAt = DateTime.UtcNow.AddMinutes(-15),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-15)
            }
        );
        db.SaveChanges();
    }
}

app.MapGet("/api/contacts", (
    int? page,
    int? pageSize,
    string? search,
    string? category,
    bool? favorite,
    string? sortBy,
    string? sortOrder,
    ContactService svc) =>
    svc.GetPagedAsync(page ?? 1, pageSize ?? 10, search, category, favorite, sortBy, sortOrder));

app.MapGet("/api/contacts/{id:guid}", async (Guid id, ContactService svc) =>
    await svc.GetByIdAsync(id) is { } contact ? Results.Ok(contact) : Results.NotFound(new { error = "Contact not found." }));

app.MapPost("/api/contacts", async (ContactRequest req, ContactService svc) =>
{
    var contact = await svc.CreateAsync(req);
    return Results.Created($"/api/contacts/{contact.Id}", contact);
});

app.MapPut("/api/contacts/{id:guid}", async (Guid id, ContactRequest req, ContactService svc) =>
    Results.Ok(await svc.UpdateAsync(id, req)));

app.MapDelete("/api/contacts/{id:guid}", async (Guid id, ContactService svc) =>
    await svc.DeleteAsync(id) ? Results.NoContent() : Results.NotFound(new { error = "Contact not found." }));

app.MapPatch("/api/contacts/{id:guid}/favorite", async (Guid id, ContactService svc) =>
    Results.Ok(await svc.ToggleFavoriteAsync(id)));

app.MapGet("/api/contacts/stats", (ContactService svc) => svc.GetStatsAsync());

app.MapGet("/api/contacts/export", async (ContactService svc) =>
    Results.File(System.Text.Encoding.UTF8.GetBytes(await svc.ExportJsonAsync()), "application/json", "contacts.json"));

app.MapPost("/api/contacts/import", async (HttpRequest request, ContactService svc) =>
{
    using var reader = new StreamReader(request.Body);
    var json = await reader.ReadToEndAsync();
    var count = await svc.ImportJsonAsync(json);
    return Results.Ok(new { imported = count });
});

app.Run();
