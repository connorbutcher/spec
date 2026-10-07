# PUSpecSheet.Database

A SQL database project (SDK-style, `Microsoft.Build.Sql`) that maps the whole `PUSpecSheet` schema as one script per object. It is a readable, buildable mirror of the database that the EF Core migrations in `api/src/PUSpecSheet.Data` create. **The migrations stay the source of truth**: nothing deploys from this project, and the scripts are generated, never edited by hand.

Last compared against migration `20261007192715_AddCheckConstraints`: no differences.

## Layout

```
PUSpecSheet.Database.sqlproj   project file; also carries the database options (collation, RCSI, ...)
dotnet-tools.json              pins SqlPackage as a local dotnet tool
refresh.ps1                    regenerate the scripts from the database
compare.ps1                    build, then compare the project with the database
dbo/Tables/*.sql               one file per table: columns, keys, constraints, indexes
values/Tables/*.sql            the typed cell-value tables in the [values] schema
Security/values.sql            the [values] schema itself
```

The scripts were produced with `SqlPackage /Action:Extract /p:ExtractTarget=SchemaObjectType` (the same engine as the SSDT schema compare) and are kept exactly as SqlPackage writes them, so a refresh only shows real schema changes in `git diff`.

`dbo.__EFMigrationsHistory` is deliberately left out: it belongs to EF, not to the schema. Both scripts handle that for you.

## Build

```bash
dotnet build database
```

This validates every script and the references between them, and writes `bin/Debug/PUSpecSheet.Database.dacpac`.

## Refresh after a new EF migration

1. Apply the migration to your local database (start the API in Development, or `dotnet ef database update`).
2. Regenerate the scripts, then check the result:

```powershell
./database/refresh.ps1
./database/compare.ps1
```

`refresh.ps1` extracts the schema read-only and replaces the script folders. `compare.ps1` builds the project and runs a SqlPackage deploy report against the database (nothing is changed), including objects that exist only in the database; it prints `No differences` or one line per difference. Update the migration name at the top of this file and commit the changed scripts with the migration.

Both scripts default to `(localdb)\MSSQLLocalDB` and database `PUSpecSheet`; pass `-Server` / `-Database` to point elsewhere. The first run restores SqlPackage with `dotnet tool restore`.

The underlying commands, if you want to run them yourself from `database/`:

```powershell
dotnet sqlpackage /Action:Extract "/SourceConnectionString:Server=(localdb)\MSSQLLocalDB;Database=PUSpecSheet;Trusted_Connection=True" /TargetFile:<empty folder> /p:ExtractTarget=SchemaObjectType
dotnet sqlpackage /Action:DeployReport /SourceFile:bin/Debug/PUSpecSheet.Database.dacpac "/TargetConnectionString:Server=(localdb)\MSSQLLocalDB;Database=PUSpecSheet;Trusted_Connection=True" /OutputPath:report.xml /p:DropObjectsNotInSource=true
```

The project also opens in Visual Studio (SSDT) and in the SQL Database Projects extension for VS Code, where the graphical schema compare works against it.

## Schema overview

32 tables in two schemas, 56 foreign keys, 76 indexes (14 filtered), 27 check constraints, 1 computed column. Every table has an `int` identity `Id` primary key unless noted.

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

All four revision tables share one shape: `RevisionNumber`, `Status` (0 draft, 1 published), `IsDeleted`, author, created/updated/published/superseded timestamps, the `SheetVersionId` they were published in, and a `ROWVERSION` column for optimistic concurrency. Check constraints tie `PublishedAtUtc` and `SupersededAtUtc` to `Status`, and filtered unique indexes allow one draft (`Status = 0`) and one current published revision (`Status = 1 AND SupersededAtUtc IS NULL`) per object.

### Values (`values` schema)

One table per value kind. Each has the composite primary key (`SheetRowRevisionId`, `SheetCellId`) and no `Id`: a value is only ever found by its revision and cell, and the table is stored in that order. Deleting a row revision cascades to its values.

| Table | Value |
| --- | --- |
| `values.TextValues` | `nvarchar(4000)`, plus the persisted computed column `LookupValue` (first 200 characters) indexed by `IX_TextValues_LookupValue` for lookups. |
| `values.NumericValues` | Numeric value. |
| `values.DateValues` | Date value. |
| `values.BooleanValues` | Boolean value. |
| `values.OptionValues` | Reference to a `dbo.CellTypeOptions` row. |

### Users, roles and permissions

| Table | Purpose |
| --- | --- |
| `dbo.Users` | Users: user name, display name, email, active flag. Referenced as author and publisher by the sheet tables. |
| `dbo.Roles` | Named roles. |
| `dbo.Permissions` | Permissions identified by `Key`. |
| `dbo.UserRoles` | Roles of a user. Composite key (`UserId`, `RoleId`). |
| `dbo.RolePermissions` | Permissions of a role. Composite key (`RoleId`, `PermissionId`). |
