# PUSpecSheet.Database

A SQL database project (SDK-style, `Microsoft.Build.Sql`) that defines the whole `PUSpecSheet` schema as one script per object. **This project is the source of truth for the schema**: the application is database first. The database is created and changed only by deploying this project; EF Core (`api/src/PUSpecSheet.Database`) has no migrations and never creates or alters anything, it only maps the `Db*` entity classes onto the tables defined here.

## Layout

```
PUSpecSheet.Database.sqlproj       project file; also carries the database options (collation, RCSI, ...)
dotnet-tools.json                  pins SqlPackage as a local dotnet tool
deploy.ps1                         build, then deploy the project to a database (creating it if needed)
compare.ps1                        build, then list what a deploy would change in a database
Script.PostDeployment.sql          reference data, run after every deployment
OneTime/*.sql                      scripts run once by hand against existing databases; not part of the build
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

42 tables in six schemas, grouped by area; nothing lives in `dbo`. Every table has an `int` identity `Id` primary key unless noted.

| Schema | Holds |
| --- | --- |
| `spec` | Sheets and everything on them: tables, sections, rows, column blocks, cells, their revisions and the published versions. |
| `design` | Table templates and their versions, sections, rows, cells and column blocks, and the cell types they use. |
| `cell` | The values entered in sheet cells, one table per kind, and the settings chosen for cells on the sheet. |
| `approval` | Drafts submitted for publishing and their approvers. |
| `auth` | Users, roles and permissions. |
| `setup` | Phases and sheet types. |

Databases created before this layout have their tables in `dbo` and `values` (or `template`, briefly). Move them once, before the next deploy, with the one-time script; it creates the schemas, moves every table with `ALTER SCHEMA ... TRANSFER` (rows, keys, indexes and constraints go with it), drops the emptied old schemas, runs in one transaction and is safe to run twice:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -d PUSpecSheet -b -i database/OneTime/MoveTablesToSchemas.sql
./database/deploy.ps1
```

Without it, a deploy would see the old tables as ones to drop and the new ones as ones to create, and stop rather than lose the data.

Sheets from the legacy app (its tables are in `api/legacy.txt`), where every version of a sheet is a whole copy of it, are moved over by the migrator in `api/tools/PUSpecSheet.Migrator`. It rebuilds each sheet's history as revisions and checks it against every copy. The legacy tables can stay where they are, as the new tables are in other schemas. Its README has the steps.

Databases deployed with `spec.Sheets.IdentityKey` (16 November 2026, sheets told apart by identifiers) need that column dropped before the next deploy, because it holds values and a deploy stops rather than drop it. Its contents are still in `spec.SheetIdentifierValues`. The one-time script is safe to run twice:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -d PUSpecSheet -b -i database/OneTime/DropSheetIdentityKey.sql
./database/deploy.ps1
```

### Phases and sheet types (`setup` schema)

| Table | Purpose |
| --- | --- |
| `setup.Phases` | Phase tree (`ParentPhaseId` self-reference), with `Code` and `DisplayOrder`. |
| `setup.SheetTypes` | Kinds of sheet (Specification, PFKs, Parts, ...). |
| `setup.SheetTypeIdentifiers` | The values that tell one sheet of a type from another within a phase (an engine specification's serial number, say): `Key` (the published API's filter name, fixed once created), `Name`, `Kind` (0 text, 1 number, 2 date, 3 option), `IsRequired`, `DisplayOrder`, and `Options` as a JSON array for the option kind. |
| `setup.SheetTypeProperties` | Values a sheet of a type records about itself (from the legacy app's sheet type properties): `Key`, `Name`, `ValueType`, `ControlType`, `DisplayOrder` and the form settings. A sheet's values are in `spec.SheetProperties`. |
| `setup.PhaseSheetTypes` | Which sheet types a phase uses. Composite key (`PhaseId`, `SheetTypeId`). |

### Templates (`design` schema)

| Table | Purpose |
| --- | --- |
| `design.TableTemplates` | A named table layout belonging to a sheet type. |
| `design.TableTemplateVersions` | Numbered versions of a template: orientation and sticky column count. |
| `design.TemplateSections` | Sections of a template version, nestable (`ParentSectionId`), with a `Role` and min/max/initial instance counts. Filtered unique index allows one `Header` section per version. |
| `design.TemplateColumnBlocks` | Repeatable groups of columns with instance limits. |
| `design.TemplateRows` | Rows within a template section. |
| `design.TemplateCells` | Cells of a template row: position, spans, caption, cell type, overrides, optional `LookupKey` (filtered index where not null). |
| `design.WidgetTypes` | Kinds of widget a table or section can show, with a fixed `Key`. |
| `design.TableTemplateWidgets` / `design.TemplateSectionWidgets` | The widgets a template version or template section shows. A sheet's values for them are JSON on the table or section revision (`WidgetValues`), by widget id. |
| `design.CellTypes` | Cell types (`Kind`, `Configuration`, `Style`). |
| `design.CellTypeOptions` | The choices of an option-kind cell type. |

### Sheets and revisions (`spec` schema)

Each sheet object is split into an identity table (stable `Id` plus a `PublicId` GUID defaulting to `newsequentialid()`) and a `...Revisions` table holding its draft and published states.

| Table | Purpose |
| --- | --- |
| `spec.Sheets` | Sheets of a phase and sheet type, indexed by (`PhaseId`, `SheetTypeId`). A sheet's identity is its values in `spec.SheetIdentifierValues` and nothing on this table; the API keeps those value sets unique per phase and type (under an application lock), and allows one sheet with no values, so a sheet type without identifiers keeps one sheet per phase. Has a `RowVersion`. |
| `spec.SheetIdentifierValues` | A sheet's value for each of its type's identifiers, set when the sheet is created and never changed; the only place a sheet's identity is stored. Composite key (`SheetId`, `SheetTypeIdentifierId`); indexed by identifier and value for the published API's filters. |
| `spec.SheetVersions` | Published versions of a sheet: number, when, by whom, note. A version migrated from the legacy app keeps the `SpecSheets` row it came from (`SourceSpecSheetId`), and that row's `LegacySpecSheetId` and `HasBeenMigrated` from the system before it. |
| `spec.SheetTables` / `spec.SheetTableRevisions` | A table on a sheet, built from a template version; revisions carry title, order, widget values and the table's notes (a JSON array: note, source system, obsolete, who and when). |
| `spec.SheetSections` / `spec.SheetSectionRevisions` | Section instances (nestable) and their revisions, which carry widget values and, for a linked section, the public id of the section it links to. |
| `spec.SheetColumnBlocks` / `spec.SheetColumnBlockRevisions` | Column block instances and their revisions. |
| `spec.SheetRows` / `spec.SheetRowRevisions` | Row instances and their revisions; cell values hang off the row revision. |
| `spec.SheetCells` | A cell of a sheet row, tied to its template cell and optional column block. |
| `spec.SheetProperties` / `spec.SheetPropertyRevisions` | The sheet's value of each sheet type property (one per property), and their revisions, which hold the `Value`. |
| `spec.SheetExternalReferences` / `spec.SheetExternalReferenceRevisions` | References pushed in by external systems, one per `ReferenceId`, and their revisions (system, owner, URL, type, grouping, icon, description), each tied to a version. |
| `spec.SheetLinkedTables` / `spec.SheetLinkedTableRevisions` | Tables from other sheets shown on this one, held by the table's public id (so the sheet may not exist yet, or any more), and their revisions, which carry the order. |

The table, section, column block, row and linked table revision tables share one shape: `RevisionNumber`, `Status` (0 draft, 1 published), `IsDeleted`, author, created/updated/published/superseded timestamps, the `SheetVersionId` they were published in, the `SheetSubmissionId` of the approval they wait on, and a `ROWVERSION` column for optimistic concurrency. Check constraints tie `PublishedAtUtc` and `SupersededAtUtc` to `Status`, and filtered unique indexes allow one draft (`Status = 0`) and one current published revision (`Status = 1 AND SupersededAtUtc IS NULL`) per object.

Property revisions have the same shape and rules, plus two more. A published revision must name its version (`CK_SheetPropertyRevisions_Version`), and a property has at most one revision per version (`UX_SheetPropertyRevisions_OnePerVersion`), so its value changes at most once from one version to the next.

External reference revisions are pushed in by the external system, not edited in the app. So they have no `Status`, author, drafts or approvals: each is published when pushed, for the `SheetVersionId` it names, which is required. They keep the same `[PublishedAtUtc, SupersededAtUtc)` timeline, with `PublishedAtUtc` set to the version's publish time and the actual arrival in `PushedAtUtc`. That way "the sheet as of a version" includes the references pushed for it and carries them forward until the reference is next pushed. A reference has one revision per version (`UX_SheetExternalReferenceRevisions_OnePerVersion`), so pushing again for the same version updates it. A push for an older version goes into the middle of the timeline: it is superseded at once by the revision after it.

### Cell values (`cell` schema)

One table per value kind. Each has the composite primary key (`SheetRowRevisionId`, `SheetCellId`) and no `Id`: a value is only ever found by its revision and cell, and the table is stored in that order. Deleting a row revision cascades to its values.

| Table | Value |
| --- | --- |
| `cell.TextValues` | `nvarchar(4000)`, plus the persisted computed column `LookupValue` (first 200 characters) indexed by `IX_TextValues_LookupValue` for lookups. |
| `cell.NumericValues` | Numeric value. |
| `cell.DateValues` | Date value. |
| `cell.BooleanValues` | Boolean value. |
| `cell.OptionValues` | Reference to a `design.CellTypeOptions` row. |
| `cell.LabelledTextValues` | One row per slot of a labelled text cell, keyed by revision, cell and `SlotKey`, with its `Position`; `LookupValue` is computed and indexed as for text. |
| `cell.LinkedSheetValues` | A cell pointing at a table, or an item in it, on another sheet: the sheet type, phase and the public ids of the table and item. |
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
