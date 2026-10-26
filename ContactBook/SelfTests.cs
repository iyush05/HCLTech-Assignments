using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public static class SelfTests
{
    public static void Run()
    {
        Console.WriteLine("Running ContactBook Self-Tests (EF Core & REST API Logic)...");

        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ContactDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var db = new ContactDbContext(options))
        {
            db.Database.EnsureCreated();

            var service = new ContactService(db);

            // 1. Create Contact
            var c1 = service.CreateAsync(new ContactRequest(
                "Ada", "Lovelace", "ada@computing.org", "+44 20 7946 0910",
                "Pioneering Computing", "Algorithm Architect", "Work", true, "First programmer."
            )).GetAwaiter().GetResult();
            Assert(c1.Id != Guid.Empty, "Contact ID should be generated");
            Assert(c1.FirstName == "Ada", "First name should match");

            // 2. Get By Id
            var fetched = service.GetByIdAsync(c1.Id).GetAwaiter().GetResult();
            Assert(fetched is not null && fetched.Email == "ada@computing.org", "Fetched contact email should match");

            // 3. Email Validation
            bool caughtBadEmail = false;
            try
            {
                service.CreateAsync(new ContactRequest("Bad", "Email", "not-an-email", "12345", null, null, "Personal", false, null)).GetAwaiter().GetResult();
            }
            catch (ContactValidationException)
            {
                caughtBadEmail = true;
            }
            Assert(caughtBadEmail, "Invalid email must throw ContactValidationException");

            // 4. Required Field Validation
            bool caughtEmptyName = false;
            try
            {
                service.CreateAsync(new ContactRequest("", "NoFirst", "valid@test.com", "12345", null, null, "Personal", false, null)).GetAwaiter().GetResult();
            }
            catch (ContactValidationException)
            {
                caughtEmptyName = true;
            }
            Assert(caughtEmptyName, "Empty first name must throw ContactValidationException");

            // 5. Add more contacts for pagination & filtering
            service.CreateAsync(new ContactRequest("Alan", "Turing", "alan@bletchley.org", "+44 1908 640404", "Government Code", "Cryptanalyst", "Work", true, null)).GetAwaiter().GetResult();
            service.CreateAsync(new ContactRequest("Grace", "Hopper", "grace@navy.mil", "+1 202 555 0184", "US Navy", "Rear Admiral", "Work", false, "Compiler pioneer")).GetAwaiter().GetResult();
            service.CreateAsync(new ContactRequest("Katherine", "Johnson", "katherine@nasa.gov", "+1 757 555 0143", "NASA", "Mathematician", "Other", false, null)).GetAwaiter().GetResult();
            service.CreateAsync(new ContactRequest("Linus", "Torvalds", "linus@linux.org", "+1 503 555 0199", "Linux Foundation", "Fellow", "Personal", false, null)).GetAwaiter().GetResult();

            // 6. Pagination (5 contacts total, pageSize 2 -> 3 pages)
            var paged = service.GetPagedAsync(page: 1, pageSize: 2).GetAwaiter().GetResult();
            Assert(paged.TotalCount == 5, $"Expected 5 total contacts, got {paged.TotalCount}");
            Assert(paged.TotalPages == 3, $"Expected 3 pages, got {paged.TotalPages}");
            Assert(paged.Items.Count == 2, $"Expected 2 items on page 1, got {paged.Items.Count}");
            Assert(paged.HasNext, "Page 1 should have next page");
            Assert(!paged.HasPrevious, "Page 1 should not have previous page");

            // 7. Search Filter
            var searchResult = service.GetPagedAsync(search: "Turing").GetAwaiter().GetResult();
            Assert(searchResult.TotalCount == 1 && searchResult.Items[0].LastName == "Turing", "Search should find Turing");

            // 8. Category Filter
            var workResult = service.GetPagedAsync(category: "Work").GetAwaiter().GetResult();
            Assert(workResult.TotalCount == 3, $"Expected 3 Work contacts, got {workResult.TotalCount}");

            // 9. Toggle Favorite
            var toggled = service.ToggleFavoriteAsync(c1.Id).GetAwaiter().GetResult();
            Assert(!toggled.IsFavorite, "Favorite should toggle to false");

            // 10. Update & Delete
            var updated = service.UpdateAsync(c1.Id, new ContactRequest(
                "Ada", "Countess of Lovelace", "ada@computing.org", "+44 20 7946 0910",
                "Pioneering Computing", "Chief Scientist", "Work", true, "Updated notes."
            )).GetAwaiter().GetResult();
            Assert(updated.LastName == "Countess of Lovelace", "Last name should update");

            var deleted = service.DeleteAsync(c1.Id).GetAwaiter().GetResult();
            Assert(deleted, "Delete should succeed");
            var remaining = service.GetByIdAsync(c1.Id).GetAwaiter().GetResult();
            Assert(remaining is null, "Deleted contact should not be found");
        }

        Console.WriteLine("All 10 ContactBook checks passed successfully.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new Exception($"Test assertion failed: {message}");
    }
}
