public sealed class Student
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public decimal Mathematics { get; set; }
    public decimal Science { get; set; }
    public decimal English { get; set; }
    public decimal Average => Math.Round((Mathematics + Science + English) / 3, 2);
    public string Grade => Average switch { >= 90 => "A", >= 80 => "B", >= 70 => "C", >= 60 => "D", _ => "F" };
}

public sealed class GradeService(JsonStore<List<Student>> store)
{
    public object List(string? search, string? sort)
    {
        var all = store.Read();
        var filtered = all.Where(s => string.IsNullOrWhiteSpace(search)
            || s.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        var students = sort == "name" ? filtered.OrderBy(s => s.Name)
            : filtered.OrderByDescending(s => s.Average).ThenBy(s => s.Name);
        return new { students = students.ToList(), total = all.Count,
            average = all.Count == 0 ? 0 : Math.Round(all.Average(s => s.Average), 2),
            passing = all.Count(s => s.Average >= 60) };
    }

    public Student Save(Guid? id, StudentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 80)
            throw new ValidationException("Enter a student name between 1 and 80 characters.");
        if (new[] { request.Mathematics, request.Science, request.English }.Any(n => n < 0 || n > 100))
            throw new ValidationException("Every subject score must be between 0 and 100.");
        return store.Update(students =>
        {
            var student = id is null ? new Student() : students.FirstOrDefault(s => s.Id == id)
                ?? throw new ValidationException("Student not found. Refresh the list.");
            student.Name = request.Name.Trim(); student.Mathematics = request.Mathematics;
            student.Science = request.Science; student.English = request.English;
            if (id is null) students.Add(student);
            return student;
        });
    }

    public bool Delete(Guid id) => store.Update(students => students.RemoveAll(s => s.Id == id) > 0);
}
public sealed record StudentRequest(string Name, decimal Mathematics, decimal Science, decimal English);
