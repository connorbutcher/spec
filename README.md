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
database/                     SQL database project mirroring the migrated schema, one script per object (see database/README.md)
```

Project references flow one way: `Api -> Application -> Data -> Domain`, with `Contracts` referenced by `Api` and `Application`. How the API is put together, and how to add an endpoint, entity or migration, is in [api/README.md](api/README.md).

## Conventions

Both linters fail on any violation, so a clean run means these standards hold.

- API (`dotnet build`; rules in `api/.editorconfig`, run in every build via `EnforceCodeStyleInBuild`): braces always (`IDE0011`), one type per file named after it (StyleCop `SA1402`, `SA1649`), file-scoped namespaces, explicit accessibility, and the `latest-recommended` .NET analyzers.
- UI (`npm run lint`; rules in `app/eslint.config.mjs` plus the project rules in `app/eslint-rules/`): braces always, one class and one interface per file, every component in `<name>/<name>.ts` with `<name>.html` and `<name>.scss` beside it, no `.component` in file names or `Component` on class names, signal inputs/outputs/queries, `httpResource` for reads and no `subscribe` in components, built-in control flow, and PrimeNG controls instead of native buttons, inputs, selects, textareas and tables.

## Running

Database: SQL Server LocalDB, database `PUSpecSheet` (connection string in `api/src/PUSpecSheet.Api/appsettings.json`). In Development the API applies migrations on startup, which also creates the database.

```bash
dotnet run --project api/src/PUSpecSheet.Api --launch-profile http
```

```bash
cd app && npm start
```

The UI runs on http://localhost:4200 and proxies `/api` and `/hubs` (the SignalR connection for multi-user editing) to the API on http://localhost:5047. To see two people on one sheet before sign-in exists, open a second tab at the same address with `?developerUser=engineer2` added (see [api/README.md](api/README.md#multi-user-editing)). The header shows whether `/api/health` (which includes a database check) is reachable.

## API documentation

In Development the API serves interactive documentation (Swagger UI) for every endpoint at http://localhost:5047/docs, built from the OpenAPI document at http://localhost:5047/openapi/v1.json. Neither is served outside Development.

Endpoint text comes from the XML comments on the controller actions (`<summary>`, `<remarks>`, `<param>`, `<response>`) and on the contracts. Everything else lives in `api/src/PUSpecSheet.Api/ApiDocumentation`: the tags and the order they are listed in (`ApiTagCatalog`), the 400/404/409 problem responses every action gets (`ProblemResponsesConvention`), and the document transformers. A new controller needs a `[Tags(ApiTags.…)]` and comments on its actions.

## Checks

```bash
dotnet build api/PUSpecSheet.slnx && dotnet format api/PUSpecSheet.slnx --verify-no-changes
```

While the API is running its `bin` folders are locked, so build into a separate folder instead:

```bash
dotnet build api/PUSpecSheet.slnx --artifacts-path api/.lint-build
```

```bash
cd app && npm run lint && npm run build && npx ng test --watch=false
```

## Migrations

```bash
dotnet ef migrations add <Name> --project api/src/PUSpecSheet.Data --startup-project api/src/PUSpecSheet.Api
```
