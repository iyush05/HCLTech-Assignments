internal static class SelfTests
{
    private static int assertions;
    public static void Run()
    {
        string directory = Path.Combine(Path.GetTempPath(), "hcl-tests-" + Guid.NewGuid());
        string path = Path.Combine(directory, "data.json");
        try
        {
            var store = new JsonStore<PasswordOptions>(path);
            var service = new PasswordService(store);
            var options = new PasswordOptions { Length = 32, Count = 10 };
            var passwords = service.Generate(options);
            Check(passwords.Length == 10 && passwords.All(p => p.Length == 32), "Requested count and length");
            Check(passwords.All(p => p.Any(char.IsUpper) && p.Any(char.IsLower) && p.Any(char.IsDigit) && p.Any(c => !char.IsLetterOrDigit(c))), "Every selected group is represented");
            Check(passwords.All(p => !p.Any(c => "O0Il1".Contains(c))), "Ambiguous characters excluded");
            Check(passwords.All(p => !File.ReadAllText(path).Contains(p)), "Passwords are not persisted");
            Check(new JsonStore<PasswordOptions>(path).Read().Length == 32, "Preferences persist");
            Reject(() => service.Generate(new PasswordOptions { Uppercase = false, Lowercase = false, Numbers = false, Symbols = false }));
            Reject(() => service.Generate(new PasswordOptions { Length = 7 }));
            var digits = service.Generate(new PasswordOptions { Uppercase = false, Lowercase = false, Symbols = false, Numbers = true });
            Check(digits.Single().All(char.IsDigit), "Only selected characters are used");
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
