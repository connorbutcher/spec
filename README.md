# PU Spec Sheet

Engine specification sheets, built from table templates and versioned by phase and sheet type.

## Layout

```
api/                                .NET solution (PUSpecSheet.slnx)
  src/PUSpecSheet.Api                 ASP.NET Core host: controllers and startup
  src/PUSpecSheet.CheckInOut.Api      SignalR host: checking sheet tables in and out
  src/PUSpecSheet.Api.Common          Authentication, authorisation, CORS, logging, Redis and Swagger set-up shared by both hosts
  src/PUSpecSheet.Api.DAL             Data access services, the change processors and the query resolvers
  src/PUSpecSheet.Api.Database        EF Core DbContext, schema classes, their configurations and mappers
  src/PUSpecSheet.Api.Emails          Notification emails
  src/PUSpecSheet.Abstractions        Domain models, requests and operations shared with the UI
  src/PUSpecSheet.Query               The query models and requests
  tools/PUSpecSheet.Migrator          Keeps the revision tables in step with the copied sheets
  tests/PUSpecSheet.Migrator.Tests    The migrator's tests
app/                                Angular UI
database/                           SQL database project: the schema, one script per object, and the SqlPackage scripts
docs/legacy-schema.txt              The database's tables and columns
```

## Running

Fill in the connection string, Azure AD and Redis settings in `appsettings.Development.json` for both hosts, then:

```bash
dotnet run --project api/src/PUSpecSheet.Api
dotnet run --project api/src/PUSpecSheet.CheckInOut.Api
```

```bash
cd app && npm install && npm start
```

## Bringing in the new app

This codebase is the base. The rewrite it replaces, with revision-based versioning, approvals and the published API, is in commit `2c82183`, and is ported onto this codebase in stages:

1. Make this codebase the repo's base. *(done)*
2. Put the database schema under source control in a SQL database project. *(done; generated from the DbContext and the schema dump, not yet compared with a live database)*
3. Add the revision tables in their own schemas, beside the existing ones. *(done: `spec`, `cell` and `approval`, built on the existing template tables)*
4. Bring across the migrator, re-runnable, and keep the revision tables in step with the existing ones. *(done: `api/tools/PUSpecSheet.Migrator`)*
5. Serve the query API and read-only screens from the revision tables. *(done)*
6. Move editing onto drafts and revisions. *(done: every sheet type is edited on drafts and revisions; the plan and slices are in `docs/PROGRESS.md`)*
7. Port the remaining features: the published API, lookups, external references, the app's screens. Templates stay as they are: the template tables and their editing aren't replaced. *(done: the query API, resolvers, admin and the Angular app all use the sheets)*
8. Retire the copied-sheet tables and the code that writes them. *(the API code is retired: the `specsheet`, `workflow` and `specsheettables` controllers, the copy readers, the read and edit switches and the copy edit processors are gone. What remains is below.)*

## Sheets

Every sheet is read and written through `ISheetDataAccessService` and the `/sheets` endpoints, in the shape of the revision schema (`Sheet`, `SheetTable`, ...). A published version of a sheet is also a spec sheet, known by the id of the `SpecSheets` row it came from (`SheetVersion.SourceSpecSheetId`), and so is a sheet's unpublished draft, known by its `SpecSheets` header row. The app opens, edits and lists by those ids, which the `/sheets/spec-sheet/{specSheetId}/...` endpoints take; the other endpoints take the sheet's id. The query API (`/query`) builds its models from the same sheets (`PUSpecSheet.Api.DAL/Query`: mappers, the table builder and the resolvers for linked tables and cells).

Published sheets that a link points at are cached (`ISpecSheetCache`, keyed by the spec sheet id of the version); the cache key prefix is `sheets:`, so entries from before the move are never read.

What remains of the copied model:

- The legacy tables and their `Db*` entities, which the migrator reads. Run it with `"commit": true` on a schedule while anything still writes the legacy tables.
- The `SpecSheets` row, which is the draft's header (lists, emails, approval steps) and the source of each version's id.
- `SpecSheetRevisionReader` and the `Abstractions/Domain/Legacy` models, which `SheetDraftCreator` still uses to copy a published sheet's tables into a new draft. Porting `SheetDraftCreator`, `SheetDraftCopier`, `SheetDraftItems` and `SheetCopySignatures` to the `Sheet` models would let both go.
- In the Angular app, the screens still work on the `SpecSheet` models, which `SheetAdapter` builds from what `/sheets` returns.

