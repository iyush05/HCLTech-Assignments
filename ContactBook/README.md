# Contact Book API

An independent ASP.NET Core 8 Web API & browser application implementing full RESTful HTTP semantics, EF Core 8 with SQLite, server-side pagination, and an interactive live HTTP API explorer console.

## Run

Install the .NET 8 SDK, or run `bash setup.sh` from the repository root. Then:

```sh
# From the repository root:
source ./dotnet-env.sh
cd ContactBook
dotnet run
```

Open **http://localhost:5111**. Press Ctrl+C to stop.
Run `bash run-all.sh` from the repository root to start all projects simultaneously (ports 5101–5111).

## Features

- **Full Contact Management**: Create, view, update, and delete contacts with first/last names, email, phone, company, role, category, and notes.
- **Server-Side Pagination & Sorting**: High-performance paging (`page`, `pageSize`, `totalCount`, `totalPages`, `hasNext`, `hasPrevious`).
- **Filtering & Search**: Instant filtering by category (Work, Personal, Family, Other), favorite stars, or search keywords across name, company, and email.
- **Dual-View Interface**:
  - **Directory View**: Intuitive card layout with colorful avatar chips, click-to-email (`mailto:`), and click-to-call (`tel:`).
  - **Live API Explorer**: Built-in HTTP console (like Swagger/Postman) allowing you to test `GET`, `POST`, `PUT`, `DELETE`, and `PATCH` endpoints with custom payloads and view real-time HTTP status codes, latency, and formatted JSON output.
- **Data Export & Import**: One-click JSON backup export and restore.
- **Entity Framework Core 8**: Database operations using SQLite (`App_Data/contacts.db`) with indexes and transactions.

## REST, HTTP & EF Core concepts

| Topic | Implementation |
| --- | --- |
| HTTP Verbs | Standard semantics: `GET` (read), `POST` (create), `PUT` (replace), `DELETE` (remove), `PATCH` (partial/state change). |
| HTTP Status Codes | `200 OK`, `201 Created` (+ Location header), `204 NoContent`, `400 BadRequest` (validation), `404 NotFound`. |
| Server Pagination | `Skip((page - 1) * pageSize).Take(pageSize)` in EF Core with `PagedResponse<T>` metadata. |
| Model Validation | Explicit checks for name constraints, email regex structure, and phone number lengths. |
| EF Core SQLite | Configured in `ContactDbContext` with indexes on `Email`, `LastName`, `Category`, and `IsFavorite`. |
| Self-Tests | `SelfTests.cs` runs 10 assertions checking CRUD, validation errors, pagination, and filtering in-memory. |

## Files

- `Program.cs`: ASP.NET Core host, Minimal API route registrations, error middleware, and SQLite DB initialization.
- `ContactModels.cs`: DTO request records, pagination models, and stats definitions.
- `ContactService.cs`: Business logic, input validation, LINQ queries, and JSON export/import.
- `ContactDbContext.cs`: EF Core `DbContext` and `Contact` entity mapping.
- `SelfTests.cs`: Executable checks for database persistence, validation rules, and pagination arithmetic.
- `wwwroot/index.html`, `styles.css`, `app.js`: Browser interface featuring Directory and Live API Explorer views.
- `Properties/launchSettings.json`: Port 5111 configuration.

## Build and self-tests

```sh
dotnet build
dotnet run -- --self-test
```
