internal static class SelfTests
{
    private static int assertions;
    public static void Run()
    {
        string directory = Path.Combine(Path.GetTempPath(), "hcl-tests-" + Guid.NewGuid());
        string path = Path.Combine(directory, "data.json");
        try
        {
            var store = new JsonStore<List<Student>>(path);
            var service = new GradeService(store);
            var student = service.Save(null, new StudentRequest("Ada", 90, 80, 100));
            Check(student.Average == 90 && student.Grade == "A", "Average and grade boundary");
            service.Save(student.Id, new StudentRequest("Ada", 60, 60, 60));
            Check(new JsonStore<List<Student>>(path).Read().Single().Grade == "D", "Edited scores persist");
            Reject(() => service.Save(null, new StudentRequest("Bad", -1, 80, 80)));
            Reject(() => service.Save(null, new StudentRequest("Bad", 80, 101, 80)));
            Check(store.Read().Count == 1, "Invalid student is not saved");
            Check(service.Delete(student.Id) && store.Read().Count == 0, "Student deletion");
            service.Save(null, new StudentRequest("Zoe", 90, 90, 90));
            service.Save(null, new StudentRequest("Ada", 50, 50, 50));
            using var summary = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(service.List(null, "average")));
            Check(summary.RootElement.GetProperty("average").GetDecimal() == 70 && summary.RootElement.GetProperty("passing").GetInt32() == 1, "LINQ class statistics");
            Check(summary.RootElement.GetProperty("students")[0].GetProperty("Name").GetString() == "Zoe", "Students sort by highest average");
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
