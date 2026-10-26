using System.Security.Cryptography;

public sealed class PasswordOptions
{
    public int Length { get; set; } = 20;
    public int Count { get; set; } = 1;
    public bool Uppercase { get; set; } = true;
    public bool Lowercase { get; set; } = true;
    public bool Numbers { get; set; } = true;
    public bool Symbols { get; set; } = true;
    public bool ExcludeAmbiguous { get; set; } = true;
}

public sealed class PasswordService(JsonStore<PasswordOptions> store)
{
    public PasswordOptions Settings() => store.Read();
    public string[] Generate(PasswordOptions options)
    {
        if (options.Length < 8 || options.Length > 64 || options.Count < 1 || options.Count > 10)
            throw new ValidationException("Choose 8–64 characters and 1–10 passwords.");
        List<string> groups = [];
        if (options.Uppercase) groups.Add("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        if (options.Lowercase) groups.Add("abcdefghijklmnopqrstuvwxyz");
        if (options.Numbers) groups.Add("0123456789");
        if (options.Symbols) groups.Add("!@#$%^&*()-_=+[]{}:;,.?");
        if (groups.Count == 0) throw new ValidationException("Select at least one character group.");
        if (options.ExcludeAmbiguous)
            groups = groups.Select(g => string.Concat(g.Where(c => !"O0Il1".Contains(c)))).ToList();
        string pool = string.Concat(groups);
        string[] passwords = Enumerable.Range(0, options.Count).Select(_ =>
        {
            // Ensure each chosen group is represented, then securely shuffle.
            var characters = groups.Select(Pick).ToList();
            while (characters.Count < options.Length) characters.Add(Pick(pool));
            for (int i = characters.Count - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (characters[i], characters[j]) = (characters[j], characters[i]);
            }
            return new string(characters.ToArray());
        }).ToArray();
        // Only preferences are persisted. Passwords are never written to disk.
        store.Update(saved => {
            saved.Length = options.Length; saved.Count = options.Count;
            saved.Uppercase = options.Uppercase; saved.Lowercase = options.Lowercase;
            saved.Numbers = options.Numbers; saved.Symbols = options.Symbols;
            saved.ExcludeAmbiguous = options.ExcludeAmbiguous;
            return true;
        });
        return passwords;
    }
    private static char Pick(string pool) => pool[RandomNumberGenerator.GetInt32(pool.Length)];
}
