Name - Ayush Kannaujiya (CSE - 1)<br>
University Roll No. - 2300270100042
# Demo / Screenshots
<img width="1470" height="923" alt="Screenshot 2026-10-26 at 17 13 57" src="https://github.com/user-attachments/assets/9948a0ce-45b8-4afb-afb7-525ce9490951" />
<img width="1470" height="923" alt="Screenshot 2026-10-26 at 17 14 16" src="https://github.com/user-attachments/assets/a4efd520-5300-49f5-9ae9-2f12317e64d1" />
<img width="1470" height="923" alt="Screenshot 2026-10-26 at 17 14 41" src="https://github.com/user-attachments/assets/7aeb55e6-5b95-4798-a2a5-0e012c6a7385" />
<img width="1470" height="923" alt="Screenshot 2026-10-26 at 17 15 10" src="https://github.com/user-attachments/assets/dea235c1-962a-4643-ab3f-f90fde87bb07" />
<img width="1470" height="923" alt="Screenshot 2026-10-26 at 17 15 23" src="https://github.com/user-attachments/assets/f3b59db4-abaa-4da5-92bd-8732786f567a" />
<img width="1470" height="923" alt="Screenshot 2026-10-26 at 17 15 36" src="https://github.com/user-attachments/assets/6117080f-965f-4d96-85b3-3b2b1d5ec69d" />
<img width="1470" height="923" alt="Screenshot 2026-10-26 at 17 15 59" src="https://github.com/user-attachments/assets/a521453e-e54c-41d3-910e-ace878c13758" />
<img width="1470" height="923" alt="Screenshot 2026-10-26 at 17 16 14" src="https://github.com/user-attachments/assets/5114a509-a92d-4488-9899-b16c3cd33357" />
<img width="1470" height="923" alt="Screenshot 2026-10-26 at 17 16 29" src="https://github.com/user-attachments/assets/d6e74a84-3900-4c64-ad17-adbba790bb43" />
<img width="1470" height="923" alt="Screenshot 2026-10-26 at 17 16 52" src="https://github.com/user-attachments/assets/583c24c4-c819-4868-8b9d-06607a107ed1" />
<img width="1470" height="923" alt="Screenshot 2026-10-26 at 17 17 07" src="https://github.com/user-attachments/assets/ec619389-c262-43d4-9f2c-feeb6dc0a8b5" />


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

