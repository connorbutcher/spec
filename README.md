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

## Cell settings chosen on the sheet

Most of what a cell is comes from its template: its cell type, and that type's configuration and style with the cell's overrides. Some settings can only be chosen once the cell is on a sheet, because they point at something on that sheet. These are **instance settings**, and they work the same way for every kind that has them:

- They are one JSON document per cell, `CellInstanceSettings`, whose `kind` names the cell kind (like `CellConfiguration`). It is stored in `values.CellSettings`, keyed by row revision and cell exactly like a value, so settings are part of the row: changing them starts the user's draft, they are published, discarded and put back with the row, they show in the change marks, and a past version shows the settings it had.
- `PUT api/sheet-rows/{id}/cell-settings` saves them. A cell whose settings change loses its value, which was chosen under the old ones. A new row starts with the settings of the row of the same kind added to its table before it.
- In the UI, a kind that has them gets a button beside its editor (`sheet-cell-settings`) that opens that kind's form in a popover.

The first kind to use them is the **linked dropdown** (`CellKind.LinkedDropdown`): each cell is pointed at a table on its sheet and one of that table's columns, and offers the values in that column. Its rules:

| Rule | Where it lives |
| --- | --- |
| Which columns can be linked to (any value except a checkbox), and what each is called | `LinkableColumns`, sent as `table.linkableColumns` |
| The choices are the column's values as the viewer sees them, top to bottom, without repeats | `LinkedOptionCollector`, filled by `SheetViewBuilder`, sent as `sheet.linkedSources` |
| A value must be one of the choices when it is saved | `LinkedDropdownValueRule` |
| The value is stored as its text (`values.TextValues`), so the published API returns the text | `CellKindExtensions.StoresText`, `PublishedKindValueReader` |
| A stored value never changes by itself. If its column's value is edited or removed, or the table is removed, the cell keeps what was picked and the sheet flags it until someone picks again | `linked-dropdown.util.ts` (`linkedWarning`) |

**To add a setting to a kind that already has them**, add a nullable property to its settings record (`LinkedDropdownInstanceSettings`) and its TypeScript interface, check it in `CellInstanceSettingsValidator`, and add its control to the kind's form. There is no migration: the settings are JSON.

**To give another kind settings**

1. API, `Domain/CellTypes/InstanceSettings`: a sealed record deriving from `CellInstanceSettings` with nullable `init` properties, a `[JsonDerivedType]` line for it on `CellInstanceSettings`, and a line in `CellInstanceSettingsCatalog.CreateEmpty`. `CellInstanceSettingsTests` fails if the two disagree.
2. API, `Application/Sheets/CellInstanceSettingsValidator`: a `case` that checks the new settings against the sheet, if there is anything to check. Storage, drafts, publishing, change history and the endpoint need nothing.
3. UI, `features/sheets/models`: an interface for the settings, joined into `cell-instance-settings.ts`. Add the kind to `hasInstanceSettings` in `features/templates/models/cell-kinds.ts`.
4. UI: a form component for the settings (copy the shape of `sheet-linked-source-form`: input `settings`, outputs `applied` and `cancelled`) and its `@case` in `sheet-cell-settings.html`, the one place a kind is mapped to its form.
5. Whatever the settings change about the cell (its choices, its limits) is read from `cell.settings` where the cell is drawn or validated.

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
