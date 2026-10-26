# Bank Account Simulator

An independent C# / ASP.NET Core 8 application with its own navy-and-mint browser
frontend in `wwwroot/`. No Node.js, frontend build step, or third-party NuGet packages.

## Run

Install the .NET 8 SDK, or run `bash setup.sh` from the repository root. Then:

```sh
# From the repository root (skip source for a system-wide SDK):
source ./dotnet-env.sh
cd BankAccountSimulator
dotnet run
```

Open **http://localhost:5107**. Ctrl+C stops the application.
Run `bash run-all.sh` from the repository root to start all eight apps; sidebar
links use fixed localhost ports 5101–5108. On Windows, use a system-wide .NET 8 SDK
and run `dotnet run` inside this directory.

## Features

- Start with a zero-balance fictional USD account.
- Deposit or withdraw an amount with an optional note.
- Track balance, total deposits/withdrawals, and the full transaction ledger.
- Record each transaction's timestamp and balance afterward.
- Reject overdrafts, negative/zero amounts, fractions smaller than a cent, and oversized transactions. Per-transaction maximum: 1,000,000; account balance maximum: 1,000,000,000.

## Data & OOP concepts

| Topic | Implementation |
| --- | --- |
| Classes | `BankAccount.Transact` encapsulates balance rules; `BankTransaction` models ledger entries. |
| Collections | `List<BankTransaction>` records successful transactions. |
| LINQ | `Where`, `Sum`, and `OrderByDescending` summarize and display account activity. |
| Files | `JsonStore<BankAccount>` saves the balance and ledger together atomically. |

## Files

- `Program.cs`: host, dependency injection, API routes, and error responses.
- `AccountService.cs`: models and application rules.
- `JsonStore.cs`: JSON reads and writes, a process-local lock, and atomic file replacement.
- `SelfTests.cs`: executable checks for rules and persistence, using temporary files.
- `wwwroot/index.html`, `styles.css`, `app.js`: standalone browser frontend.
- `Properties/launchSettings.json`: the project's localhost port.

Runtime data is created at `App_Data/account.json` after the first successful write.
It is outside `wwwroot`, ignored by Git, and survives page/server restarts. The app
is a local learning project: browser tabs share the same data; run only one server
instance against a given data directory. Existing corrupt files are reported as
errors rather than silently overwritten. File changes within one process are
serialized; no database or account sign-in is required.

## Build and self-tests

From this directory, with the SDK available:

```sh
dotnet build
dotnet run -- --self-test
```

Self-tests run without opening a server port. They use temporary data and do not
change your saved app data.

## Browser checks

1. Deposit 100.25 and withdraw 40.10; expect balance 60.15.
2. Try withdrawing 100; expect rejection with the balance and ledger unchanged.
3. Try zero, a negative amount, or 1.001; expect rejection.
4. Reload/restart; the ledger and balance should remain.
5. Check transaction notes, totals, and balance-after values. This app handles fictional funds only.

Browser rendering and HTTP checks could not be run in the creation environment
because local server sockets are restricted. Build, service tests, and static
frontend checks are run separately.
