# Quiz App

An independent C# / ASP.NET Core 8 application with its own navy-and-mint browser
frontend in `wwwroot/`. No Node.js, frontend build step, or third-party NuGet packages.

## Run

Install the .NET 8 SDK, or run `bash setup.sh` from the repository root. Then:

```sh
# From the repository root (skip source for a system-wide SDK):
source ./dotnet-env.sh
cd QuizApp
dotnet run
```

Open **http://localhost:5106**. Ctrl+C stops the application.
Run `bash run-all.sh` from the repository root to start all eight apps; sidebar
links use fixed localhost ports 5101–5108. On Windows, use a system-wide .NET 8 SDK
and run `dotnet run` inside this directory.

## Features

- Answer six C# questions from the editable `Questions.json` bank.
- Move backward and forward, change answers, and track progress.
- Submit all answers for server-side scoring and an explanation of each answer.
- View saved attempts, the best score, and the ten most recent rounds.
- Retry the quiz. In-progress selections stay in the page only and reset on reload.

## Data & OOP concepts

| Topic | Implementation |
| --- | --- |
| Classes | `Question`, `QuizAttempt`, and `QuizService` separate content, results, and scoring. |
| Collections | A question list and `Dictionary<int, int>` map question IDs to answer indexes. |
| LINQ | `Count`, `Select`, `Max`, and `OrderByDescending` score and summarize attempts. |
| Files | Read `Questions.json`; persist results with `JsonStore<List<QuizAttempt>>`. |

## Files

- `Program.cs`: host, dependency injection, API routes, and error responses.
- `QuizService.cs`: models and application rules.
- `JsonStore.cs`: JSON reads and writes, a process-local lock, and atomic file replacement.
- `SelfTests.cs`: executable checks for rules and persistence, using temporary files.
- `wwwroot/index.html`, `styles.css`, `app.js`: standalone browser frontend.
- `Properties/launchSettings.json`: the project's localhost port.

Runtime data is created at `App_Data/attempts.json` after the first successful write.
It is outside `wwwroot`, ignored by Git, and survives page/server restarts. The app
is a local learning project: browser tabs share the same data; run only one server
instance against a given data directory. Existing corrupt files are reported as
errors rather than silently overwritten. File changes within one process are
serialized; no database or account sign-in is required.

The question bank is copied when building/publishing. Keep question IDs unique, use
zero-based `Answer` indexes, and restart after editing it. Changing the bank does not
rewrite old attempt scores; remove `App_Data/attempts.json` with the app stopped if
you want a fresh history for a different quiz.

## Build and self-tests

From this directory, with the SDK available:

```sh
dotnet build
dotnet run -- --self-test
```

Self-tests run without opening a server port. They use temporary data and do not
change your saved app data.

## Browser checks

1. Choose an answer, advance, go back, and verify the selection is retained.
2. Finish the quiz; check the score and review explanations.
3. Retry with different answers; history and best score should update.
4. Reload/restart to confirm completed attempts persist.
5. The API must reject missing/out-of-range answers. Correct answers are excluded from the questions endpoint.

Browser rendering and HTTP checks could not be run in the creation environment
because local server sockets are restricted. Build, service tests, and static
frontend checks are run separately.
