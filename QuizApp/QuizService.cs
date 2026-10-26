using System.Text.Json;

public sealed class Question
{
    public int Id { get; set; }
    public string Prompt { get; set; } = "";
    public string[] Options { get; set; } = [];
    public int Answer { get; set; }
    public string Explanation { get; set; } = "";
}
public sealed class QuizAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
    public int Score { get; set; }
    public int Total { get; set; }
}
public sealed class QuizService
{
    private readonly List<Question> questions;
    private readonly JsonStore<List<QuizAttempt>> store;
    public QuizService(string questionPath, JsonStore<List<QuizAttempt>> store)
    {
        this.store = store;
        questions = JsonSerializer.Deserialize<List<Question>>(File.ReadAllText(questionPath))
            ?? throw new IOException("The question file is empty.");
        if (questions.Count == 0 || questions.Select(q => q.Id).Distinct().Count() != questions.Count
            || questions.Any(q => q.Options.Length < 2 || q.Answer < 0 || q.Answer >= q.Options.Length))
            throw new IOException("The question file contains invalid questions.");
    }
    public object Questions() => questions.Select(q => new { q.Id, q.Prompt, q.Options }).ToList();
    public object History()
    {
        var attempts = store.Read();
        return new { attempts = attempts.OrderByDescending(a => a.Date).Take(10).ToList(),
            total = attempts.Count, best = attempts.Count == 0 ? 0 : attempts.Max(a => a.Score) };
    }
    public object Submit(Dictionary<int, int>? answers)
    {
        if (answers is null || answers.Count != questions.Count
            || questions.Any(q => !answers.TryGetValue(q.Id, out int answer) || answer < 0 || answer >= q.Options.Length))
            throw new ValidationException("Answer every question before submitting.");
        var attempt = new QuizAttempt { Score = questions.Count(q => answers[q.Id] == q.Answer), Total = questions.Count };
        store.Update(attempts => { attempts.Add(attempt); return true; });
        return new { attempt, review = questions.Select(q => new {
            q.Prompt, selected = q.Options[answers[q.Id]], correct = q.Options[q.Answer],
            isCorrect = answers[q.Id] == q.Answer, q.Explanation
        }) };
    }
}
