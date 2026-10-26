# Password Generator

An independent C# / ASP.NET Core 8 application with its own navy-and-mint browser
frontend in `wwwroot/`. No Node.js, frontend build step, or third-party NuGet packages.

## Run

Install the .NET 8 SDK, or run `bash setup.sh` from the repository root. Then:

```sh
# From the repository root (skip source for a system-wide SDK):
source ./dotnet-env.sh
cd PasswordGenerator
dotnet run
```

Open **http://localhost:5108**. Ctrl+C stops the application.
Run `bash run-all.sh` from the repository root to start all eight apps; sidebar
links use fixed localhost ports 5101–5108. On Windows, use a system-wide .NET 8 SDK
and run `dotnet run` inside this directory.

## Features

- Choose 8–64 characters and generate 1–10 passwords at a time.
- Select uppercase, lowercase, numbers, and/or symbols.
- Optionally exclude `O`, `0`, `I`, `l`, and `1`.
- Guarantee at least one character from every selected group.
- Copy one password or all results; clear results from the page.
- Remember preferences only. Passwords are generated using `RandomNumberGenerator` and are never written to a file or added to application logs.

## Data & OOP concepts

| Topic | Implementation |
| --- | --- |
| Classes | `PasswordOptions` models preferences; `PasswordService` generates passwords. |
| Collections | Character-group lists and character arrays build each password. |
| LINQ | `Where`, `Select`, and `Enumerable.Range` filter groups and generate batches. |
| Files | `JsonStore<PasswordOptions>` remembers preferences without storing passwords. |

## Files

- `Program.cs`: host, dependency injection, API routes, and error responses.
- `PasswordService.cs`: models and application rules.
- `JsonStore.cs`: JSON reads and writes, a process-local lock, and atomic file replacement.
- `SelfTests.cs`: executable checks for rules and persistence, using temporary files.
- `wwwroot/index.html`, `styles.css`, `app.js`: standalone browser frontend.
- `Properties/launchSettings.json`: the project's localhost port.

Runtime data is created at `App_Data/preferences.json` after the first successful write.
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

1. Generate 10 passwords of length 32 with all groups selected; check length and groups.
2. Enable ambiguous-character exclusion; check that O, 0, I, l, and 1 never appear.
3. Select only Numbers; outputs should contain only digits.
4. Deselect every group; expect a validation message.
5. Copy results (requires browser clipboard permission), then clear them.
6. Reload/restart; settings remain but generated results do not. Clearing results does not clear the clipboard.

Browser rendering and HTTP checks could not be run in the creation environment
because local server sockets are restricted. Build, service tests, and static
frontend checks are run separately.
