# PUSpecSheet.Database

A SQL database project (SDK-style, `Microsoft.Build.Sql`) that defines the whole `PUSpecSheet` schema as one script per object. **This project is the source of truth for the schema**: the application is database first. The database is created and changed only by deploying this project; EF Core (`api/src/PUSpecSheet.Database`) has no migrations and never creates or alters anything, it only maps the `Db*` entity classes onto the tables defined here.

## Layout

```
PUSpecSheet.Database.sqlproj   project file; also carries the database options (collation, RCSI, ...)
dotnet-tools.json              pins SqlPackage as a local dotnet tool
deploy.ps1                     build, then deploy the project to a database (creating it if needed)
compare.ps1                    build, then list what a deploy would change in a database
Script.PostDeployment.sql      reference data, run after every deployment
dbo/Tables/*.sql               one file per table: columns, keys, constraints, indexes
values/Tables/*.sql            the typed cell-value tables in the [values] schema
Security/values.sql            the [values] schema itself
```

The table scripts are in the layout SqlPackage writes (`/Action:Extract /p:ExtractTarget=SchemaObjectType`), so keep new ones in the same style.

## Build

```bash
dotnet build database
```

This validates every script and the references between them, and writes `bin/Debug/PUSpecSheet.Database.dacpac`.

## Deploy

```powershell
./database/deploy.ps1
```

Builds the project and publishes it with SqlPackage, which creates the database if it doesn't exist, makes whatever changes bring it in line with the project and then runs `Script.PostDeployment.sql`. A change that would lose data stops the deployment. `compare.ps1` shows what a deploy would change without changing anything; it prints `No differences` or one line per difference.

Both scripts default to `(localdb)\MSSQLLocalDB` and database `PUSpecSheet`; pass `-Server` / `-Database` to point elsewhere. The first run restores SqlPackage with `dotnet tool restore`.

The project also opens in Visual Studio (SSDT) and in the SQL Database Projects extension for VS Code, where publishing and the graphical schema compare work against it.

## Changing the schema

1. Change the scripts here (add a column to the table's script, add a new file for a new table, ...) and `dotnet build database`.
2. Deploy to your local database with `./database/deploy.ps1`.
3. Mirror the change in EF Core: the `Db*` entity in `api/src/PUSpecSheet.Database/Models/<Area>` and its configuration in `Configurations/<Area>`. Column names, types, lengths, keys, defaults and relationships there must match the scripts.

## Reference data

`Script.PostDeployment.sql` adds the rows every database needs: the sheet types, the starting cell types and the Pass / Fail options, one permission per key in `PermissionKeys`, the Administrator role and the developer user who holds it. It runs after every deployment and only inserts missing rows, so edits made in the app to cell types or sheet types are kept. Permissions are kept in step with the code: their key and description are updated on every deployment. `PermissionSeedTests` checks the script has a row for every `PermissionKeys` constant.

## Schema overview

36 tables in two schemas. Every table has an `int` identity `Id` primary key unless noted.

### Phases and sheet types

| Table | Purpose |
| --- | --- |
| `dbo.Phases` | Phase tree (`ParentPhaseId` self-reference), with `Code` and `DisplayOrder`. |
| `dbo.SheetTypes` | Kinds of sheet (Specification, PFKs, Parts, ...). |
| `dbo.PhaseSheetTypes` | Which sheet types a phase uses. Composite key (`PhaseId`, `SheetTypeId`). |

### Templates

| Table | Purpose |
| --- | --- |
| `dbo.TableTemplates` | A named table layout belonging to a sheet type. |
| `dbo.TableTemplateVersions` | Numbered versions of a template: orientation and sticky column count. |
| `dbo.TemplateSections` | Sections of a template version, nestable (`ParentSectionId`), with a `Role` and min/max/initial instance counts. Filtered unique index allows one `Header` section per version. |
| `dbo.TemplateColumnBlocks` | Repeatable groups of columns with instance limits. |
| `dbo.TemplateRows` | Rows within a template section. |
| `dbo.TemplateCells` | Cells of a template row: position, spans, caption, cell type, overrides, optional `LookupKey` (filtered index where not null). |
| `dbo.CellTypes` | Cell types (`Kind`, `Configuration`, `Style`). |
| `dbo.CellTypeOptions` | The choices of an option-kind cell type. |

### Sheets and revisions

Each sheet object is split into an identity table (stable `Id` plus a `PublicId` GUID defaulting to `newsequentialid()`) and a `...Revisions` table holding its draft and published states.

| Table | Purpose |
| --- | --- |
| `dbo.Sheets` | One sheet per phase and sheet type. Has a `RowVersion`. |
| `dbo.SheetVersions` | Published versions of a sheet: number, when, by whom, note. |
| `dbo.SheetTables` / `dbo.SheetTableRevisions` | A table on a sheet, built from a template version; revisions carry title and order. |
| `dbo.SheetSections` / `dbo.SheetSectionRevisions` | Section instances (nestable) and their revisions. |
| `dbo.SheetColumnBlocks` / `dbo.SheetColumnBlockRevisions` | Column block instances and their revisions. |
| `dbo.SheetRows` / `dbo.SheetRowRevisions` | Row instances and their revisions; cell values hang off the row revision. |
| `dbo.SheetCells` | A cell of a sheet row, tied to its template cell and optional column block. |

All four revision tables share one shape: `RevisionNumber`, `Status` (0 draft, 1 published), `IsDeleted`, author, created/updated/published/superseded timestamps, the `SheetVersionId` they were published in, the `SheetSubmissionId` of the approval they wait on, and a `ROWVERSION` column for optimistic concurrency. Check constraints tie `PublishedAtUtc` and `SupersededAtUtc` to `Status`, and filtered unique indexes allow one draft (`Status = 0`) and one current published revision (`Status = 1 AND SupersededAtUtc IS NULL`) per object.

### Values (`values` schema)

One table per value kind. Each has the composite primary key (`SheetRowRevisionId`, `SheetCellId`) and no `Id`: a value is only ever found by its revision and cell, and the table is stored in that order. Deleting a row revision cascades to its values.

| Table | Value |
| --- | --- |
| `values.TextValues` | `nvarchar(4000)`, plus the persisted computed column `LookupValue` (first 200 characters) indexed by `IX_TextValues_LookupValue` for lookups. |
| `values.NumericValues` | Numeric value. |
| `values.DateValues` | Date value. |
| `values.BooleanValues` | Boolean value. |
| `values.OptionValues` | Reference to a `dbo.CellTypeOptions` row. |
| `values.LabelledTextValues` | One row per slot of a labelled text cell, keyed by revision, cell and `SlotKey`, with its `Position`; `LookupValue` is computed and indexed as for text. |
| `values.CellSettings` | The settings chosen on the sheet for a cell (which table a linked dropdown reads, for example), as JSON whose `kind` names the cell kind. Same key as the value tables, so they are versioned with the row. |

### Approvals

| Table | Purpose |
| --- | --- |
| `dbo.SheetSubmissions` | Drafts submitted for publishing: who, when, note, `Status` (0 pending, 1 approved, 2 rejected, 3 withdrawn) and the `SheetVersionId` it published. Has a `RowVersion`. |
| `dbo.SheetSubmissionApprovers` | The approvers chosen for a submission and each one's `Decision` (a rejection needs a `Reason`). Composite key (`SheetSubmissionId`, `UserId`). |

### Users, roles and permissions

| Table | Purpose |
| --- | --- |
| `dbo.Users` | Users: user name, display name, email, active flag. Referenced as author and publisher by the sheet tables. |
| `dbo.Roles` | Named roles. |
| `dbo.Permissions` | Permissions identified by `Key`. |
| `dbo.UserRoles` | Roles of a user. Composite key (`UserId`, `RoleId`). |
| `dbo.RolePermissions` | Permissions of a role. Composite key (`RoleId`, `PermissionId`). |
