using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public static class SelfTests
{
    public static void Run()
    {
        Console.WriteLine("Running PersonalNotes Self-Tests (EF Core & SQLite)...");

        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<NotesDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var db = new NotesDbContext(options))
        {
            db.Database.EnsureCreated();

            var service = new NoteService(db);

            // 1. Add Note
            var note1 = service.AddAsync(new NoteRequest("Architecture Plan", "Drafting clean microservices with ASP.NET Core.", "Work", "mint", false)).GetAwaiter().GetResult();
            Assert(note1.Id != Guid.Empty, "Note ID should be generated.");
            Assert(note1.Title == "Architecture Plan", "Title should match.");

            // 2. Add Pinned Note
            var note2 = service.AddAsync(new NoteRequest("Shopping List", "Fresh coffee, oat milk, honey.", "Personal", "amber", true)).GetAwaiter().GetResult();
            Assert(note2.IsPinned, "Note 2 should be pinned.");

            // 3. List and Ordering (pinned should be first)
            dynamic result = service.ListAsync(null, null, null).GetAwaiter().GetResult();
            var list = (List<Note>)result.notes;
            Assert(list.Count == 2, $"Expected 2 notes, got {list.Count}");
            Assert(list[0].Id == note2.Id, "Pinned note must appear first.");

            // 4. Search
            dynamic searchResult = service.ListAsync(null, "coffee", null).GetAwaiter().GetResult();
            var searchList = (List<Note>)searchResult.notes;
            Assert(searchList.Count == 1 && searchList[0].Title == "Shopping List", "Search should find 'coffee'");

            // 5. Category filter
            dynamic categoryResult = service.ListAsync("Work", null, null).GetAwaiter().GetResult();
            var categoryList = (List<Note>)categoryResult.notes;
            Assert(categoryList.Count == 1 && categoryList[0].Category == "Work", "Category filter should return Work notes");

            // 6. Update
            var updated = service.UpdateAsync(note1.Id, new NoteRequest("Architecture Plan v2", "Updated content with EF Core.", "Work", "sky", true)).GetAwaiter().GetResult();
            Assert(updated.Title == "Architecture Plan v2", "Title should be updated.");
            Assert(updated.IsPinned, "Note should now be pinned.");

            // 7. Toggle Pin
            var toggled = service.TogglePinAsync(note1.Id).GetAwaiter().GetResult();
            Assert(!toggled.IsPinned, "Pin should now be toggled off.");

            // 8. Stats
            var stats = service.GetStatsAsync().GetAwaiter().GetResult();
            Assert(stats.Total == 2, $"Expected 2 total notes, got {stats.Total}");
            Assert(stats.Pinned == 1, $"Expected 1 pinned note, got {stats.Pinned}");

            // 9. Validation error
            bool caughtValidation = false;
            try
            {
                service.AddAsync(new NoteRequest("", "No title provided", "General", "mint", false)).GetAwaiter().GetResult();
            }
            catch (ValidationException)
            {
                caughtValidation = true;
            }
            Assert(caughtValidation, "Empty title should throw ValidationException.");

            // 10. Delete
            var deleted = service.DeleteAsync(note1.Id).GetAwaiter().GetResult();
            Assert(deleted, "Delete should return true.");
            var remainingStats = service.GetStatsAsync().GetAwaiter().GetResult();
            Assert(remainingStats.Total == 1, "Expected 1 note remaining.");
        }

        Console.WriteLine("All 10 PersonalNotes checks passed successfully.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new Exception($"Test assertion failed: {message}");
    }
}
