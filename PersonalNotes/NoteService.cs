using Microsoft.EntityFrameworkCore;

public class ValidationException(string message) : Exception(message);

public record NoteRequest(string? Title, string? Content, string? Category, string? Color, bool IsPinned);

public record NoteStats(int Total, int Pinned, int CategoriesCount, int TotalWords);

public sealed class NoteService(NotesDbContext db)
{
    private static readonly HashSet<string> AllowedColors = new(StringComparer.OrdinalIgnoreCase)
    {
        "mint", "amber", "sky", "rose", "lavender", "emerald"
    };

    public async Task<object> ListAsync(string? category, string? search, bool? pinnedOnly)
    {
        var query = db.Notes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(n => n.Category == category.Trim());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(n => EF.Functions.Like(n.Title, $"%{term}%") || EF.Functions.Like(n.Content, $"%{term}%"));
        }

        if (pinnedOnly == true)
        {
            query = query.Where(n => n.IsPinned);
        }

        var notes = await query
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.UpdatedAt)
            .ToListAsync();

        var stats = await GetStatsAsync();
        return new { notes, stats };
    }

    public async Task<Note?> GetByIdAsync(Guid id)
    {
        return await db.Notes.FindAsync(id);
    }

    public async Task<Note> AddAsync(NoteRequest request)
    {
        Validate(request.Title, request.Content, request.Category, request.Color);

        var note = new Note
        {
            Title = request.Title!.Trim(),
            Content = request.Content!.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? "General" : request.Category.Trim(),
            Color = NormalizeColor(request.Color),
            IsPinned = request.IsPinned,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Notes.Add(note);
        await db.SaveChangesAsync();
        return note;
    }

    public async Task<Note> UpdateAsync(Guid id, NoteRequest request)
    {
        Validate(request.Title, request.Content, request.Category, request.Color);

        var note = await db.Notes.FindAsync(id)
            ?? throw new ValidationException("Note not found or may have been deleted.");

        note.Title = request.Title!.Trim();
        note.Content = request.Content!.Trim();
        note.Category = string.IsNullOrWhiteSpace(request.Category) ? "General" : request.Category.Trim();
        note.Color = NormalizeColor(request.Color);
        note.IsPinned = request.IsPinned;
        note.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return note;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var note = await db.Notes.FindAsync(id);
        if (note is null) return false;

        db.Notes.Remove(note);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<Note> TogglePinAsync(Guid id)
    {
        var note = await db.Notes.FindAsync(id)
            ?? throw new ValidationException("Note not found or may have been deleted.");

        note.IsPinned = !note.IsPinned;
        note.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return note;
    }

    public async Task<NoteStats> GetStatsAsync()
    {
        var all = await db.Notes.AsNoTracking().ToListAsync();
        var total = all.Count;
        var pinned = all.Count(n => n.IsPinned);
        var categoriesCount = all.Select(n => n.Category).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        var totalWords = all.Sum(n => CountWords(n.Content));
        return new NoteStats(total, pinned, categoriesCount, totalWords);
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        var categories = await db.Notes.AsNoTracking()
            .Select(n => n.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        if (!categories.Contains("General"))
        {
            categories.Insert(0, "General");
        }
        return categories;
    }

    private static void Validate(string? title, string? content, string? category, string? color)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 120)
            throw new ValidationException("Note title is required and must be between 1 and 120 characters.");

        if (string.IsNullOrWhiteSpace(content) || content.Trim().Length > 5000)
            throw new ValidationException("Note content is required and must not exceed 5000 characters.");

        if (!string.IsNullOrWhiteSpace(category) && category.Trim().Length > 40)
            throw new ValidationException("Category name cannot exceed 40 characters.");

        if (!string.IsNullOrWhiteSpace(color) && !AllowedColors.Contains(color.Trim()))
            throw new ValidationException($"Invalid color theme. Choose one of: {string.Join(", ", AllowedColors)}.");
    }

    private static string NormalizeColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color) || !AllowedColors.Contains(color.Trim()))
            return "mint";
        return color.Trim().ToLowerInvariant();
    }

    private static int CountWords(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}
