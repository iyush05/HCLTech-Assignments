# Form & Function — C# everyday toolkit

Eleven independent ASP.NET Core 8 projects with matching responsive browser UIs.
Every project contains its own C# backend, frontend assets, and run instructions.
Projects 1–3 also retain their original console exercises.

## Run all eleven apps

Requires the .NET 8 SDK. On macOS/Linux, install a local SDK if needed:

```sh
bash setup.sh
```

Start all applications:

```sh
bash run-all.sh
```

Keep the terminal running. Ctrl+C stops the apps. Open any of these URLs:

| # | Project | Browser URL | Instructions | Focus |
| --- | --- | --- | --- | --- |
| 1 | Number Guessing Game | http://localhost:5101 | [README](NumberGuessingGame/README.md) | Syntax & loops |
| 2 | Simple Calculator | http://localhost:5102 | [README](SimpleCalculator/README.md) | Conditionals & methods |
| 3 | Unit Converter | http://localhost:5103 | [README](UnitConverter/README.md) | Data conversions |
| 4 | To-Do List | http://localhost:5104 | [README](ToDoList/README.md) | OOP & JSON store |
| 5 | Student Grade Manager | http://localhost:5105 | [README](StudentGradeManager/README.md) | LINQ & collections |
| 6 | Quiz App | http://localhost:5106 | [README](QuizApp/README.md) | Models & state |
| 7 | Bank Account Simulator | http://localhost:5107 | [README](BankAccountSimulator/README.md) | Domain logic & audit |
| 8 | Password Generator | http://localhost:5108 | [README](PasswordGenerator/README.md) | Security & preferences |
| 9 | Personal Notes Web App | http://localhost:5109 | [README](PersonalNotes/README.md) | ASP.NET Core, EF Core, SQLite |
| 10 | Weather App | http://localhost:5110 | [README](WeatherApp/README.md) | HTTP Client, REST APIs, EF Core |
| 11 | Contact Book API | http://localhost:5111 | [README](ContactBook/README.md) | REST APIs, Pagination, API Console |

Sidebar navigation connects all eleven local URLs. To run just one app:

```sh
source ./dotnet-env.sh
dotnet run --project PersonalNotes
```

Skip `source` with a system-wide SDK. On Windows, install the .NET 8 SDK and run
`dotnet run --project <project-name>` in separate terminals instead of using the
shell scripts.

## What you will learn

- **Projects 1–3 (Core Fundamentals):** syntax, loops, conditions, methods, and input validation.
- **Projects 4–8 (Data & OOP):** model/service classes, collections, LINQ queries, dependency
  injection, and JSON file persistence.
- **Projects 9–11 (Web & APIs):** ASP.NET Core Minimal APIs, Entity Framework Core 8, SQLite databases,
  typed `HttpClient` and external REST API consumption, resilience fallbacks, RFC status codes,
  server pagination, and an interactive in-browser HTTP API explorer.

Every app has a `Program.cs` host and a standalone `wwwroot/` containing
`index.html`, `styles.css`, and `app.js`. Projects 4–11 add service/model classes and `SelfTests.cs`.
Projects 9–11 use Entity Framework Core 8 with SQLite for relational persistence in `App_Data/`.

## Build and verify

```sh
source ./dotnet-env.sh
dotnet build PersonalNotes
dotnet run --project PersonalNotes -- --self-test
dotnet run --project WeatherApp -- --self-test
dotnet run --project ContactBook -- --self-test
```

Each new project supports `--self-test` to check its C# business rules and EF Core persistence with
in-memory SQLite connections, without opening server ports.
