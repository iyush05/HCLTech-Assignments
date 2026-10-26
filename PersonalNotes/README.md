# Personal Notes Web App

An independent ASP.NET Core 8 Web API & browser application using Entity Framework Core 8 and SQLite.
Features its own navy-and-mint responsive frontend in `wwwroot/`.

## Run

Install the .NET 8 SDK, or run `bash setup.sh` from the repository root. Then:

```sh
# From the repository root:
source ./dotnet-env.sh
cd PersonalNotes
dotnet run
```

Open **http://localhost:5109**. Press Ctrl+C to stop.
Run `bash run-all.sh` from the repository root to start all projects simultaneously (ports 5101–5111).

## Features

- **Full Note Management**: Add, update, view, and delete rich notes with custom category tags and color accents.
- **Top Pinning**: Pin crucial notes to stay at the top of your workspace.
- **Instant Search & Filters**: Live filtering by keywords, categories, or pinned status.
- **Notebook Telemetry**: Real-time stats on total notes, pinned items, distinct categories, and total word count.
- **Entity Framework Core 8**: Database operations using EF Core `DbContext`, `DbSet<Note>`, LINQ queries, and index definitions.
- **Local SQLite Storage**: Saves persistent data in `App_Data/notes.db`.

## Web & EF Core concepts

| Topic | Implementation |
| --- | --- |
| Minimal APIs | `app.MapGet`, `app.MapPost`, `app.MapPut`, `app.MapDelete`, `app.MapPatch` for clean REST routing. |
| EF Core DbContext | `NotesDbContext` manages database connection, entity mapping, and schema configuration. |
| SQLite Provider | `Microsoft.EntityFrameworkCore.Sqlite` stores notes in a robust, portable embedded file. |
| LINQ to Entities | `AsNoTracking()`, `EF.Functions.Like()`, `Where()`, `OrderByDescending()`, and async execution (`ToListAsync()`). |
| Database Initialization | `Database.EnsureCreated()` creates the database schema and sample starter records on startup. |
| Self-Tests | `SelfTests.cs` runs 10 automated assertions against an in-memory SQLite instance without binding to network ports. |

## Files

- `Program.cs`: ASP.NET Core host, EF Core service registration, SQLite path config, error middleware, and API routes.
- `NotesDbContext.cs`: EF Core `DbContext` and `Note` model with indexes and constraints.
- `NoteService.cs`: Business logic, input validation, and asynchronous database queries.
- `SelfTests.cs`: Executable checks for CRUD, search, ordering, pin toggle, validation, and statistics.
- `wwwroot/index.html`, `styles.css`, `app.js`: Browser interface matching the Form & Function design system.
- `Properties/launchSettings.json`: Port 5109 configuration.

## Build and self-tests

```sh
dotnet build
dotnet run -- --self-test
```
