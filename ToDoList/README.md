# To-Do List

An independent C# / ASP.NET Core 8 application with its own navy-and-mint browser
frontend in `wwwroot/`. No Node.js, frontend build step, or third-party NuGet packages.

## Run

Install the .NET 8 SDK, or run `bash setup.sh` from the repository root. Then:

```sh
# From the repository root (skip source for a system-wide SDK):
source ./dotnet-env.sh
cd ToDoList
dotnet run
```

Open **http://localhost:5104**. Ctrl+C stops the application.
Run `bash run-all.sh` from the repository root to start all eight apps; sidebar
links use fixed localhost ports 5101–5108. On Windows, use a system-wide .NET 8 SDK
and run `dotnet run` inside this directory.

## Features

- Add tasks with Low, Normal, or High priority.
- Edit titles/priorities, mark tasks complete, or delete them after confirmation.
- Search by title and filter All / To do / Completed.
- View total, active, and completed counts. High-priority active tasks appear first.

## Data & OOP concepts

| Topic | Implementation |
| --- | --- |
| Classes | `TodoItem` models a task; `TaskService` owns task operations. |
| Collections | `List<TodoItem>` holds the task list. |
| LINQ | `Where`, `Count`, `OrderBy`, and `ThenByDescending` filter and prioritize tasks. |
| Files | `JsonStore<List<TodoItem>>` saves the list as JSON. |

## Files

- `Program.cs`: host, dependency injection, API routes, and error responses.
- `TaskService.cs`: models and application rules.
- `JsonStore.cs`: JSON reads and writes, a process-local lock, and atomic file replacement.
- `SelfTests.cs`: executable checks for rules and persistence, using temporary files.
- `wwwroot/index.html`, `styles.css`, `app.js`: standalone browser frontend.
- `Properties/launchSettings.json`: the project's localhost port.

Runtime data is created at `App_Data/tasks.json` after the first successful write.
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

1. Add a High-priority task and a Normal-priority task; verify their ordering.
2. Edit a title, complete a task, and check all three filters and counts.
3. Search with different capitalization; matching is case-insensitive.
4. Reload the page or restart the app; tasks should remain.
5. Delete a task and verify it disappears. Blank/overlong titles are rejected.

Browser rendering and HTTP checks could not be run in the creation environment
because local server sockets are restricted. Build, service tests, and static
frontend checks are run separately.
