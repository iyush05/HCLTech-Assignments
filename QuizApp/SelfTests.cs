internal static class SelfTests
{
    private static int assertions;
    public static void Run()
    {
        string directory = Path.Combine(Path.GetTempPath(), "hcl-tests-" + Guid.NewGuid());
        string path = Path.Combine(directory, "data.json");
        try
        {
            var store = new JsonStore<List<QuizAttempt>>(path);
            var service = new QuizService(Path.Combine(AppContext.BaseDirectory, "Questions.json"), store);
            var questions = System.Text.Json.JsonSerializer.Deserialize<List<Question>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Questions.json")))!;
            Reject(() => service.Submit(new Dictionary<int, int>()));
            Check(store.Read().Count == 0, "Incomplete attempt is not recorded");
            service.Submit(questions.ToDictionary(q => q.Id, q => q.Answer));
            Check(new JsonStore<List<QuizAttempt>>(path).Read().Single().Score == questions.Count, "Correct scoring and attempt persistence");
            service.Submit(questions.ToDictionary(q => q.Id, q => (q.Answer + 1) % q.Options.Length));
            Check(store.Read().Last().Score == 0, "Incorrect answers score zero");
            var publicQuestions = System.Text.Json.JsonSerializer.Serialize(service.Questions());
            Check(!publicQuestions.Contains("Answer") && !publicQuestions.Contains("Explanation"), "Question endpoint does not expose answers");
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
