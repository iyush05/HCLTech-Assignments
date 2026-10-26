internal static class SelfTests
{
    private static int assertions;
    public static void Run()
    {
        string directory = Path.Combine(Path.GetTempPath(), "hcl-tests-" + Guid.NewGuid());
        string path = Path.Combine(directory, "data.json");
        try
        {
            var store = new JsonStore<List<TodoItem>>(path);
            var service = new TaskService(store);
            var task = service.Add("  Review LINQ  ", "High");
            Check(task.Title == "Review LINQ", "Title trimming");
            service.Update(task.Id, "Review LINQ", "Normal", true);
            Check(new JsonStore<List<TodoItem>>(path).Read().Single().Completed, "Completion persists across store instances");
            Reject(() => service.Add("  ", "Low"));
            Reject(() => service.Add("Task", "Unknown"));
            Check(store.Read().Count == 1, "Invalid writes leave data unchanged");
            Check(service.Delete(task.Id) && store.Read().Count == 0, "Delete persists");
            service.Add("Learn collections", "Normal");
            service.Add("Read files", "High");
            using var filtered = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(service.List("active", "READ")));
            Check(filtered.RootElement.GetProperty("tasks").GetArrayLength() == 1, "Case-insensitive search and active filter");
            using var sorted = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(service.List("all", null)));
            Check(sorted.RootElement.GetProperty("tasks")[0].GetProperty("Priority").GetString() == "High", "High-priority tasks sort first");
            Console.WriteLine($"PASS: {assertions} checks.");
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true); }
    }
    private static void Check(bool passed, string name)
    {
        if (!passed) throw new Exception("Failed: " + name);
        assertions++;
    }
    private static void Reject(Action action)
    {
        try { action(); }
        catch (ValidationException) { assertions++; return; }
        throw new Exception("Expected input validation to reject the request.");
    }
}
