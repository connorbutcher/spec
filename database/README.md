# PUSpecSheet.Database

A SQL database project (SDK-style, `Microsoft.Build.Sql`) that defines the whole `PUSpecSheet` schema as one script per object. **This project is the source of truth for the schema**: the application is database first. The database is created and changed only by deploying this project; EF Core (`api/src/PUSpecSheet.Database`) has no migrations and never creates or alters anything, it only maps the `Db*` entity classes onto the tables defined here.

## Layout

```
PUSpecSheet.Database.sqlproj       project file; also carries the database options (collation, RCSI, ...)
dotnet-tools.json                  pins SqlPackage as a local dotnet tool
deploy.ps1                         build, then deploy the project to a database (creating it if needed)
compare.ps1                        build, then list what a deploy would change in a database
Script.PostDeployment.sql          reference data, run after every deployment
PUSpecSheet.Database.refactorlog   table moves between schemas, so a deploy keeps their rows
<schema>/Tables/*.sql              one file per table: columns, keys, constraints, indexes
Security/<schema>.sql              the schemas themselves
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
3. Mirror the change in EF Core: the `Db*` entity in `api/src/PUSpecSheet.Database/Models/<Area>` and its configuration in `Configurations/<Area>`. Column names, types, lengths, keys, defaults, relationships and unique indexes there must match the scripts; check constraints and plain indexes are only in the scripts.

## Reference data

`Script.PostDeployment.sql` adds the rows every database needs: the sheet types, the starting cell types and the Pass / Fail options, one permission per key in `PermissionKeys`, the Administrator role and the developer user who holds it. It runs after every deployment and only inserts missing rows, so edits made in the app to cell types or sheet types are kept. Permissions are kept in step with the code: their key and description are updated on every deployment. `PermissionSeedTests` checks the script has a row for every `PermissionKeys` constant.

## Schema overview

36 tables in six schemas, grouped by area; nothing lives in `dbo`. Every table has an `int` identity `Id` primary key unless noted.

| Schema | Holds |
| --- | --- |
| `spec` | Sheets and everything on them: tables, sections, rows, column blocks, cells, their revisions and the published versions. |
| `template` | Table templates and their versions, sections, rows, cells and column blocks, and the cell types they use. |
| `cell` | The values entered in sheet cells, one table per kind, and the settings chosen for cells on the sheet. |
| `approval` | Drafts submitted for publishing and their approvers. |
| `auth` | Users, roles and permissions. |
| `setup` | Phases and sheet types. |

Existing databases had these tables in `dbo` and `values`. `PUSpecSheet.Database.refactorlog` records each move, so deploying to one of them moves the tables with their rows instead of dropping and recreating them; the post-deployment script then drops the empty `values` schema.

### Phases and sheet types (`setup` schema)

| Table | Purpose |
| --- | --- |
| `setup.Phases` | Phase tree (`ParentPhaseId` self-reference), with `Code` and `DisplayOrder`. |
| `setup.SheetTypes` | Kinds of sheet (Specification, PFKs, Parts, ...). |
| `setup.PhaseSheetTypes` | Which sheet types a phase uses. Composite key (`PhaseId`, `SheetTypeId`). |

### Templates (`template` schema)

| Table | Purpose |
| --- | --- |
| `template.TableTemplates` | A named table layout belonging to a sheet type. |
| `template.TableTemplateVersions` | Numbered versions of a template: orientation and sticky column count. |
| `template.TemplateSections` | Sections of a template version, nestable (`ParentSectionId`), with a `Role` and min/max/initial instance counts. Filtered unique index allows one `Header` section per version. |
| `template.TemplateColumnBlocks` | Repeatable groups of columns with instance limits. |
| `template.TemplateRows` | Rows within a template section. |
| `template.TemplateCells` | Cells of a template row: position, spans, caption, cell type, overrides, optional `LookupKey` (filtered index where not null). |
| `template.CellTypes` | Cell types (`Kind`, `Configuration`, `Style`). |
| `template.CellTypeOptions` | The choices of an option-kind cell type. |

### Sheets and revisions (`spec` schema)

Each sheet object is split into an identity table (stable `Id` plus a `PublicId` GUID defaulting to `newsequentialid()`) and a `...Revisions` table holding its draft and published states.

| Table | Purpose |
| --- | --- |
| `spec.Sheets` | One sheet per phase and sheet type. Has a `RowVersion`. |
| `spec.SheetVersions` | Published versions of a sheet: number, when, by whom, note. |
| `spec.SheetTables` / `spec.SheetTableRevisions` | A table on a sheet, built from a template version; revisions carry title and order. |
| `spec.SheetSections` / `spec.SheetSectionRevisions` | Section instances (nestable) and their revisions. |
| `spec.SheetColumnBlocks` / `spec.SheetColumnBlockRevisions` | Column block instances and their revisions. |
| `spec.SheetRows` / `spec.SheetRowRevisions` | Row instances and their revisions; cell values hang off the row revision. |
| `spec.SheetCells` | A cell of a sheet row, tied to its template cell and optional column block. |

All four revision tables share one shape: `RevisionNumber`, `Status` (0 draft, 1 published), `IsDeleted`, author, created/updated/published/superseded timestamps, the `SheetVersionId` they were published in, the `SheetSubmissionId` of the approval they wait on, and a `ROWVERSION` column for optimistic concurrency. Check constraints tie `PublishedAtUtc` and `SupersededAtUtc` to `Status`, and filtered unique indexes allow one draft (`Status = 0`) and one current published revision (`Status = 1 AND SupersededAtUtc IS NULL`) per object.

### Cell values (`cell` schema)

One table per value kind. Each has the composite primary key (`SheetRowRevisionId`, `SheetCellId`) and no `Id`: a value is only ever found by its revision and cell, and the table is stored in that order. Deleting a row revision cascades to its values.

| Table | Value |
| --- | --- |
| `cell.TextValues` | `nvarchar(4000)`, plus the persisted computed column `LookupValue` (first 200 characters) indexed by `IX_TextValues_LookupValue` for lookups. |
| `cell.NumericValues` | Numeric value. |
| `cell.DateValues` | Date value. |
| `cell.BooleanValues` | Boolean value. |
| `cell.OptionValues` | Reference to a `template.CellTypeOptions` row. |
| `cell.LabelledTextValues` | One row per slot of a labelled text cell, keyed by revision, cell and `SlotKey`, with its `Position`; `LookupValue` is computed and indexed as for text. |
| `cell.CellSettings` | The settings chosen on the sheet for a cell (which table a linked dropdown reads, for example), as JSON whose `kind` names the cell kind. Same key as the value tables, so they are versioned with the row. |

### Approvals (`approval` schema)

| Table | Purpose |
| --- | --- |
| `approval.SheetSubmissions` | Drafts submitted for publishing: who, when, note, `Status` (0 pending, 1 approved, 2 rejected, 3 withdrawn) and the `SheetVersionId` it published. Has a `RowVersion`. |
| `approval.SheetSubmissionApprovers` | The approvers chosen for a submission and each one's `Decision` (a rejection needs a `Reason`). Composite key (`SheetSubmissionId`, `UserId`). |

### Users, roles and permissions (`auth` schema)

| Table | Purpose |
| --- | --- |
| `auth.Users` | Users: user name, display name, email, active flag. Referenced as author and publisher by the sheet tables. |
| `auth.Roles` | Named roles. |
| `auth.Permissions` | Permissions identified by `Key`. |
| `auth.UserRoles` | Roles of a user. Composite key (`UserId`, `RoleId`). |
| `auth.RolePermissions` | Permissions of a role. Composite key (`RoleId`, `PermissionId`). |
