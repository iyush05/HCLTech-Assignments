# Student Grade Manager

An independent C# / ASP.NET Core 8 application with its own navy-and-mint browser
frontend in `wwwroot/`. No Node.js, frontend build step, or third-party NuGet packages.

## Run

Install the .NET 8 SDK, or run `bash setup.sh` from the repository root. Then:

```sh
# From the repository root (skip source for a system-wide SDK):
source ./dotnet-env.sh
cd StudentGradeManager
dotnet run
```

Open **http://localhost:5105**. Ctrl+C stops the application.
Run `bash run-all.sh` from the repository root to start all eight apps; sidebar
links use fixed localhost ports 5101–5108. On Windows, use a system-wide .NET 8 SDK
and run `dotnet run` inside this directory.

## Features

- Add, edit, and delete student records with Mathematics, Science, and English scores.
- Search names and sort by highest average or alphabetically.
- See class average, student count, and passing count.
- Calculate letter grades from an equally weighted, two-decimal average: A ≥90, B ≥80, C ≥70, D ≥60, otherwise F. Passing means average ≥60.

## Data & OOP concepts

| Topic | Implementation |
| --- | --- |
| Classes | `Student` exposes calculated `Average` and `Grade`; `GradeService` manages records. |
| Collections | `List<Student>` stores the class roster. |
| LINQ | `Average`, `Count`, `Where`, and `OrderByDescending` create summaries and sorted views. |
| Files | `JsonStore<List<Student>>` persists scores to JSON. |

## Files

- `Program.cs`: host, dependency injection, API routes, and error responses.
- `GradeService.cs`: models and application rules.
- `JsonStore.cs`: JSON reads and writes, a process-local lock, and atomic file replacement.
- `SelfTests.cs`: executable checks for rules and persistence, using temporary files.
- `wwwroot/index.html`, `styles.css`, `app.js`: standalone browser frontend.
- `Properties/launchSettings.json`: the project's localhost port.

Runtime data is created at `App_Data/students.json` after the first successful write.
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

1. Add a student with 90, 80, and 100; expect average 90.00 and grade A.
2. Add a student with all scores 50; expect class average 70.00 and one passing student.
3. Edit a score and confirm summaries change; test name search and both sort orders.
4. Try scores outside 0–100 or a blank name; they should be rejected.
5. Restart the app to check persistence, then delete a record.

Browser rendering and HTTP checks could not be run in the creation environment
because local server sockets are restricted. Build, service tests, and static
frontend checks are run separately.
