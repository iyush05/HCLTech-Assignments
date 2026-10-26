internal static class SelfTests
{
    private static int assertions;
    public static void Run()
    {
        string directory = Path.Combine(Path.GetTempPath(), "hcl-tests-" + Guid.NewGuid());
        string path = Path.Combine(directory, "data.json");
        try
        {
            var store = new JsonStore<BankAccount>(path);
            var service = new AccountService(store);
            service.Transact(new TransactionRequest("deposit", 100.25m, "Opening deposit"));
            service.Transact(new TransactionRequest("withdraw", 40.10m, "Lunch"));
            Check(new JsonStore<BankAccount>(path).Read().Balance == 60.15m, "Exact decimal balance persists");
            Reject(() => service.Transact(new TransactionRequest("withdraw", 100m, null)));
            Reject(() => service.Transact(new TransactionRequest("deposit", -1m, null)));
            Reject(() => service.Transact(new TransactionRequest("deposit", 1.001m, null)));
            Check(store.Read().Transactions.Count == 2 && store.Read().Balance == 60.15m, "Rejected transaction leaves ledger unchanged");
            Parallel.For(0, 10, _ => service.Transact(new TransactionRequest("deposit", 1m, null)));
            Check(store.Read().Balance == 70.15m && store.Read().Transactions.Count == 12, "Concurrent deposits are serialized");
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
