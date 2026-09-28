# PU Spec Sheet

Engine specification sheets: sections (Specification, PFKs, Parts, ...) that group limits, each holding template-driven tables.

## Layout

```
api/                          .NET 10 solution (PUSpecSheet.slnx)
  src/PUSpecSheet.Api           ASP.NET Core host: controllers, middleware, startup
  src/PUSpecSheet.Application   Application services / use cases
  src/PUSpecSheet.Contracts     Request/response DTOs shared with the UI
  src/PUSpecSheet.Data          EF Core DbContext, entity configurations, migrations (SQL Server)
  src/PUSpecSheet.Domain        Domain entities and enums
app/                          Angular 22 UI (PrimeNG)
```

Project references flow one way: `Api -> Application -> Data -> Domain`, with `Contracts` referenced by `Api` and `Application`.

## Conventions

- One class / interface per file, braces always.
- API: `IDE0011` (braces) and file-scoped namespaces are build errors via `.editorconfig` + `EnforceCodeStyleInBuild`.
- UI: signals for state, `httpResource` / `resource` for data fetching, small single-purpose components. ESLint enforces `curly`, one class per file, one interface per file, and explicit member accessibility.

## Running

Database: SQL Server LocalDB, database `PUSpecSheet` (connection string in `api/src/PUSpecSheet.Api/appsettings.json`). In Development the API applies migrations on startup, which also creates the database.

```bash
dotnet run --project api/src/PUSpecSheet.Api --launch-profile http
```

```bash
cd app && npm start
```

The UI runs on http://localhost:4200 and proxies `/api` to the API on http://localhost:5047. The header shows whether `/api/health` (which includes a database check) is reachable.

## Checks

```bash
dotnet build api/PUSpecSheet.slnx && dotnet format api/PUSpecSheet.slnx --verify-no-changes
```

```bash
cd app && npm run lint && npm run build && npx ng test --watch=false
```

## Migrations

```bash
dotnet ef migrations add <Name> --project api/src/PUSpecSheet.Data --startup-project api/src/PUSpecSheet.Api
```
