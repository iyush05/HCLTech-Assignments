using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

public sealed partial class ContactService(ContactDbContext db)
{
    private static readonly HashSet<string> AllowedCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "Personal", "Work", "Family", "Other"
    };

    public async Task<PagedResponse<Contact>> GetPagedAsync(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        string? category = null,
        bool? favoritesOnly = null,
        string? sortBy = "name",
        string? sortOrder = "asc")
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Contacts.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(c => c.Category == category.Trim());
        }

        if (favoritesOnly == true)
        {
            query = query.Where(c => c.IsFavorite);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                EF.Functions.Like(c.FirstName, $"%{term}%") ||
                EF.Functions.Like(c.LastName, $"%{term}%") ||
                EF.Functions.Like(c.Email, $"%{term}%") ||
                EF.Functions.Like(c.PhoneNumber, $"%{term}%") ||
                (c.Company != null && EF.Functions.Like(c.Company, $"%{term}%")));
        }

        var isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        query = sortBy?.ToLowerInvariant() switch
        {
            "recent" => isDesc ? query.OrderBy(c => c.CreatedAt) : query.OrderByDescending(c => c.CreatedAt),
            "company" => isDesc ? query.OrderByDescending(c => c.Company).ThenBy(c => c.LastName) : query.OrderBy(c => c.Company).ThenBy(c => c.LastName),
            _ => isDesc ? query.OrderByDescending(c => c.FirstName).ThenByDescending(c => c.LastName) : query.OrderBy(c => c.FirstName).ThenBy(c => c.LastName)
        };

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        if (totalPages == 0) totalPages = 1;

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<Contact>(
            items,
            totalCount,
            page,
            pageSize,
            totalPages,
            page < totalPages,
            page > 1
        );
    }

    public async Task<Contact?> GetByIdAsync(Guid id)
    {
        return await db.Contacts.FindAsync(id);
    }

    public async Task<Contact> CreateAsync(ContactRequest req)
    {
        Validate(req);

        var contact = new Contact
        {
            FirstName = req.FirstName!.Trim(),
            LastName = req.LastName!.Trim(),
            Email = req.Email!.Trim().ToLowerInvariant(),
            PhoneNumber = req.PhoneNumber!.Trim(),
            Company = req.Company?.Trim(),
            JobTitle = req.JobTitle?.Trim(),
            Category = NormalizeCategory(req.Category),
            IsFavorite = req.IsFavorite,
            Notes = req.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Contacts.Add(contact);
        await db.SaveChangesAsync();
        return contact;
    }

    public async Task<Contact> UpdateAsync(Guid id, ContactRequest req)
    {
        Validate(req);

        var contact = await db.Contacts.FindAsync(id)
            ?? throw new ContactValidationException("Contact not found or may have been deleted.");

        contact.FirstName = req.FirstName!.Trim();
        contact.LastName = req.LastName!.Trim();
        contact.Email = req.Email!.Trim().ToLowerInvariant();
        contact.PhoneNumber = req.PhoneNumber!.Trim();
        contact.Company = req.Company?.Trim();
        contact.JobTitle = req.JobTitle?.Trim();
        contact.Category = NormalizeCategory(req.Category);
        contact.IsFavorite = req.IsFavorite;
        contact.Notes = req.Notes?.Trim();
        contact.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return contact;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var contact = await db.Contacts.FindAsync(id);
        if (contact is null) return false;

        db.Contacts.Remove(contact);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<Contact> ToggleFavoriteAsync(Guid id)
    {
        var contact = await db.Contacts.FindAsync(id)
            ?? throw new ContactValidationException("Contact not found or may have been deleted.");

        contact.IsFavorite = !contact.IsFavorite;
        contact.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return contact;
    }

    public async Task<ContactStats> GetStatsAsync()
    {
        var all = await db.Contacts.AsNoTracking().ToListAsync();
        return new ContactStats(
            Total: all.Count,
            Favorites: all.Count(c => c.IsFavorite),
            WorkCount: all.Count(c => string.Equals(c.Category, "Work", StringComparison.OrdinalIgnoreCase)),
            PersonalCount: all.Count(c => string.Equals(c.Category, "Personal", StringComparison.OrdinalIgnoreCase)),
            FamilyCount: all.Count(c => string.Equals(c.Category, "Family", StringComparison.OrdinalIgnoreCase)),
            OtherCount: all.Count(c => string.Equals(c.Category, "Other", StringComparison.OrdinalIgnoreCase))
        );
    }

    public async Task<string> ExportJsonAsync()
    {
        var all = await db.Contacts.AsNoTracking().OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToListAsync();
        return JsonSerializer.Serialize(all, new JsonSerializerOptions { WriteIndented = true });
    }

    public async Task<int> ImportJsonAsync(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ContactValidationException("Import payload cannot be empty.");

        List<Contact>? list;
        try
        {
            list = JsonSerializer.Deserialize<List<Contact>>(json);
        }
        catch (JsonException ex)
        {
            throw new ContactValidationException($"Invalid JSON format: {ex.Message}");
        }

        if (list is null || list.Count == 0) return 0;

        int added = 0;
        foreach (var c in list)
        {
            if (string.IsNullOrWhiteSpace(c.FirstName) || string.IsNullOrWhiteSpace(c.LastName) || string.IsNullOrWhiteSpace(c.Email))
                continue;

            c.Id = Guid.NewGuid();
            c.CreatedAt = DateTime.UtcNow;
            c.UpdatedAt = DateTime.UtcNow;
            db.Contacts.Add(c);
            added++;
        }

        await db.SaveChangesAsync();
        return added;
    }

    private static void Validate(ContactRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.FirstName) || req.FirstName.Trim().Length > 60)
            throw new ContactValidationException("First name is required and must not exceed 60 characters.");

        if (string.IsNullOrWhiteSpace(req.LastName) || req.LastName.Trim().Length > 60)
            throw new ContactValidationException("Last name is required and must not exceed 60 characters.");

        if (string.IsNullOrWhiteSpace(req.Email) || req.Email.Trim().Length > 120 || !IsValidEmail(req.Email.Trim()))
            throw new ContactValidationException("A valid email address is required (e.g. name@example.com).");

        if (string.IsNullOrWhiteSpace(req.PhoneNumber) || req.PhoneNumber.Trim().Length > 30)
            throw new ContactValidationException("Phone number is required and must not exceed 30 characters.");

        if (!string.IsNullOrWhiteSpace(req.Company) && req.Company.Trim().Length > 100)
            throw new ContactValidationException("Company name cannot exceed 100 characters.");

        if (!string.IsNullOrWhiteSpace(req.JobTitle) && req.JobTitle.Trim().Length > 100)
            throw new ContactValidationException("Job title cannot exceed 100 characters.");

        if (!string.IsNullOrWhiteSpace(req.Notes) && req.Notes.Trim().Length > 500)
            throw new ContactValidationException("Notes cannot exceed 500 characters.");
    }

    private static bool IsValidEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        return atIndex > 0 && atIndex < email.LastIndexOf('.') && email.LastIndexOf('.') < email.Length - 1;
    }

    private static string NormalizeCategory(string? cat)
    {
        if (string.IsNullOrWhiteSpace(cat) || !AllowedCategories.Contains(cat.Trim()))
            return "Personal";
        return char.ToUpper(cat.Trim()[0]) + cat.Trim()[1..].ToLowerInvariant();
    }
}
