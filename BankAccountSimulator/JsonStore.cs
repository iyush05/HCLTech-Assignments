using System.Text.Json;

// One store instance per app. A lock keeps read/modify/write operations together.
public sealed class JsonStore<T> where T : new()
{
    private readonly string path;
    private readonly object gate = new();
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    public JsonStore(string path) { this.path = path; }
    public T Read() { lock (gate) return ReadCore(); }
    private T ReadCore() => File.Exists(path)
        ? JsonSerializer.Deserialize<T>(File.ReadAllText(path), Options)
            ?? throw new IOException("The saved data is empty or invalid.")
        : new T();
    public TResult Update<TResult>(Func<T, TResult> change)
    {
        lock (gate)
        {
            T data = ReadCore();
            TResult result = change(data);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            string temporary = path + ".tmp";
            File.WriteAllText(temporary, JsonSerializer.Serialize(data, Options));
            File.Move(temporary, path, overwrite: true);
            return result;
        }
    }
}
public sealed class ValidationException(string message) : Exception(message);
