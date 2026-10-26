public sealed class TodoItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Priority { get; set; } = "Normal";
    public bool Completed { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public sealed class TaskService(JsonStore<List<TodoItem>> store)
{
    public object List(string? filter, string? search)
    {
        var all = store.Read();
        var tasks = all.Where(t => filter switch
        {
            "active" => !t.Completed, "completed" => t.Completed, _ => true
        }).Where(t => string.IsNullOrWhiteSpace(search)
            || t.Title.Contains(search, StringComparison.OrdinalIgnoreCase))
            .OrderBy(t => t.Completed).ThenByDescending(t => t.Priority == "High")
            .ThenByDescending(t => t.CreatedAt).ToList();
        return new { tasks, total = all.Count, completed = all.Count(t => t.Completed),
            active = all.Count(t => !t.Completed) };
    }

    public TodoItem Add(string? title, string? priority)
    {
        Validate(title, priority);
        return store.Update(tasks =>
        {
            var task = new TodoItem { Title = title!.Trim(), Priority = priority! };
            tasks.Add(task);
            return task;
        });
    }

    public TodoItem Update(Guid id, string? title, string? priority, bool completed)
    {
        Validate(title, priority);
        return store.Update(tasks =>
        {
            var task = tasks.FirstOrDefault(t => t.Id == id)
                ?? throw new ValidationException("That task no longer exists. Refresh the list.");
            task.Title = title!.Trim(); task.Priority = priority!; task.Completed = completed;
            return task;
        });
    }

    public bool Delete(Guid id) => store.Update(tasks => tasks.RemoveAll(t => t.Id == id) > 0);

    private static void Validate(string? title, string? priority)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 120)
            throw new ValidationException("Give your task a title between 1 and 120 characters.");
        if (priority is not ("Low" or "Normal" or "High"))
            throw new ValidationException("Choose Low, Normal, or High priority.");
    }
}
