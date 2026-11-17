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
5. Serve the query API and read-only screens from the revision tables, checked against the existing answers first. *(done: the `SheetReads:Source` switch, below)*
6. Move editing onto drafts and revisions one sheet type at a time, behind a per-type switch.
7. Port the remaining features one at a time: the published API, lookups, presence. Templates stay as they are: the template tables and their editing aren't replaced.
8. Retire the copied-sheet tables and the code that writes them.

## Reading published sheets from the revisions

Every whole-sheet read goes through `SpecSheetDataAccessService.GetSpecSheetByIdAsync`: the query API (by id, and the latest version of a type and phase), linked tables and linked cells, and the screens. Where it loads a sheet, on a cache miss, the `SheetReads:Source` setting decides where a published sheet comes from:

| Source | |
| --- | --- |
| `Copies` | The copy (`SpecSheets` and the tables under it), as before. The default. |
| `Compare` | The copy, which is what is returned, and the revisions too; a difference is logged as a warning naming the sheet. |
| `Revisions` | The revisions: the version migrated from that copy, as it stood when it was published (`SpecSheetRevisionReader`). A copy with no version yet, or an unpublished one, is read from the copy. |

The revision read builds the same `DbSpecSheet` a copy is read into and maps it with the same mappers, so the cache, table permissions and the query mapping after it don't change.

To move over:

1. Run the migrator with `--commit`, and keep running it on a schedule so new publishes reach the revisions.
2. Call `GET /revisions/compare` (optionally `?specSheetTypeId=` and `?specSheetPhaseId=`). It reads every published sheet both ways, without the cache, and lists the sheets that differ, with the lines each read has that the other hasn't, and those not migrated yet. Fix what it finds, in the migrator or the reader, until the list is empty.
3. Set `SheetReads:Source` to `Compare` for a while to catch what the check missed, then to `Revisions`. Reset the cache (`DELETE /cache/reset?specSheetTypeId=`) after changing it, as cached sheets keep whichever read made them.

What is left out of the comparison, because it differs by design:

- Ids of tables, groups, rows and cells (the revisions' items have their own), and of widgets, notes and workflow step actors, which the revisions number from 1 within the sheet. Items are matched by ReferenceID.
- Who created and last changed each table, group, row and cell, which the copies keep per item. From the revisions they are who wrote the item's first revision and the one shown.

What can differ and will show up if it does: a cell's own `RowNumber`, type or configuration where it isn't its row's or its template cell's (the migrator warns about these), a version's workflow steps where a copy's steps started before the version before it was published, an approver's decided time where the copy had none, and a group linked to a deleted group.

Summaries (lists of versions and their properties) and the analytics dataset still read the copies.

