public sealed class BankTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
    public string Type { get; set; } = "";
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Note { get; set; } = "";
}

public sealed class BankAccount
{
    public string Name { get; set; } = "Everyday account";
    public decimal Balance { get; set; }
    public List<BankTransaction> Transactions { get; set; } = [];

    // Encapsulates the account rules so every caller gets the same validation.
    public BankTransaction Transact(string type, decimal amount, string? note)
    {
        if (type is not ("deposit" or "withdraw")) throw new ValidationException("Choose deposit or withdrawal.");
        if (amount <= 0 || amount > 1_000_000 || decimal.Round(amount, 2) != amount)
            throw new ValidationException("Enter an amount from 0.01 to 1,000,000, with at most two decimal places.");
        if ((note?.Length ?? 0) > 120) throw new ValidationException("Keep the note under 121 characters.");
        if (type == "withdraw" && amount > Balance) throw new ValidationException("There are not enough funds for that withdrawal.");
        if (type == "deposit" && Balance > 1_000_000_000 - amount)
            throw new ValidationException("The simulated account balance limit is 1,000,000,000.");
        Balance += type == "deposit" ? amount : -amount;
        var transaction = new BankTransaction { Type = type, Amount = amount, BalanceAfter = Balance, Note = note?.Trim() ?? "" };
        Transactions.Add(transaction);
        return transaction;
    }
}

public sealed class AccountService(JsonStore<BankAccount> store)
{
    public object Summary() { var account = store.Read(); return new {
        account.Name, account.Balance,
        deposits = account.Transactions.Where(t => t.Type == "deposit").Sum(t => t.Amount),
        withdrawals = account.Transactions.Where(t => t.Type == "withdraw").Sum(t => t.Amount),
        transactions = account.Transactions.OrderByDescending(t => t.Date).ToList()
    }; }
    public BankTransaction Transact(TransactionRequest request) =>
        store.Update(account => account.Transact(request.Type, request.Amount, request.Note));
}
public sealed record TransactionRequest(string Type, decimal Amount, string? Note);
