# PUSpecSheet.Database

The database's schema, kept as a SQL database project (Microsoft.Build.Sql): one script per table, with its keys, constraints and indexes, under `<schema>/Tables/`, and one per schema under `Security/`. The `legacy` schema's are in `legacy/Tables/`, beside the legacy project (see below). Building it checks every object and reference and produces a dacpac, which SqlPackage compares with or deploys to a database.

The API reads and writes the database through EF Core (`api/src/PUSpecSheet.Api.Database`) but doesn't own the schema: there are no EF migrations. This project is the record of what the schema is. Change it here, deploy it, then update the EF schema classes and configurations to match.

## Scripts

All three restore SqlPackage as a local dotnet tool and take `-Server` and `-Database` (Windows authentication, defaulting to `(localdb)\MSSQLLocalDB` and `PUSpecSheet`) or `-ConnectionString` for anything else.

| Script | What it does |
| --- | --- |
| `extract.ps1` | Writes a database's schema into the project, replacing the schema folders (in `legacy/`, only the `Tables` folder). Review the result with git before committing it. |
| `compare.ps1` | Builds the project and lists what deploying it would change in a database, objects only in the database included. Changes nothing. Exits with 1 when there are differences. |
| `deploy.ps1` | Builds the project and makes a database match it, creating the database if needed. Stops instead of losing data. |

Database options, users, logins and permissions are kept out of the project and left alone by all three, since they differ between environments.

`copy.ps1` is the odd one out: it copies a whole database, schema and data, into a new one (a bacpac export and import), so it takes `-SourceDatabase` and `-TargetDatabase` (or `-SourceServer`, `-TargetServer` and connection strings) and doesn't use the project at all.

`seed.ps1` runs `seed.sql` with sqlcmd to give an empty database the reference data it needs (see [Seeding an empty database](#seeding-an-empty-database)). It takes `-Server` and `-Database` (Windows authentication, defaulting to `(localdb)\MSSQLLocalDB` and `PUSpecSheetLegacy`). `seed.sql` is data, not schema, so the project leaves it out of the build.

## Setting up a database

The project holds the schema only, with no data. The real types, phases, templates, cell types and users come from the legacy database itself, so a working database starts from a copy of the legacy one. (`seed.ps1` gives an empty database just enough to try the app; see [Without a legacy database](#without-a-legacy-database).)

1. Copy the legacy database (production, or a test copy of it) into a new database. Don't use the name `PUSpecSheet` on LocalDB if it is already taken by another database:

   ```powershell
   ./copy.ps1 -SourceConnectionString '<legacy database>' -TargetDatabase PUSpecSheetLegacy
   ```

   A `.bak` restored in SSMS works as well.
2. List how the copy differs from the project. Expect the `spec`, `cell` and `approval` tables to be missing, the sheet copies to be moved to `legacy` if the copy has them in `dbo`, `values`, `references` and `workflow` (see [The legacy schema](#the-legacy-schema)), and expect constraint and index names to differ (see below):

   ```powershell
   ./compare.ps1 -Database PUSpecSheetLegacy
   ```

   Anything else it lists is a gap in the project. Settle it before going on.
3. Add the revision tables beside the copies, moving the copies to `legacy` if they aren't there yet. The deployment stops rather than lose data:

   ```powershell
   ./deploy.ps1 -Database PUSpecSheetLegacy
   ```

4. Build the revisions from the copies. Do a dry run first, then commit:

   ```powershell
   dotnet run --project api/tools/PUSpecSheet.Migrator -- --connection "<connection string>"
   dotnet run --project api/tools/PUSpecSheet.Migrator -- --connection "<connection string>" --commit
   ```

5. Point `ConnectionStrings:PUSpecSheet` in `api/src/PUSpecSheet.Api/appsettings.Development.json` at the database.

Once one database is set up, `copy.ps1` makes more that are identical to it, data included, without repeating the steps.

### Without a legacy database

When there is no legacy database to copy, build one from the scripts. `legacy/PUSpecSheet.Legacy.Database.sqlproj` is the legacy schema on its own: it has no table scripts of its own but builds this project's scripts minus the revision tables (`spec`, `cell`, `approval`), so the two can't drift apart. The `legacy` schema's scripts live in its folder, `legacy/Tables/`. `deploy.ps1 -Legacy` deploys it:

```powershell
./deploy.ps1 -Database PUSpecSheetLegacy -Legacy   # the 40 legacy tables
./deploy.ps1 -Database PUSpecSheetLegacy           # adds the 22 revision tables beside them
./compare.ps1 -Database PUSpecSheetLegacy          # should report no differences
```

The result has the whole schema and no rows. Nothing in it works until the reference data is there: users and permissions (`access`), phases, sheet types and their properties, structure levels and widget types (`types`), cell types, table templates and their groups, rows, cells and widgets (`template`), and cell configurations. Sheets, values and revisions start empty as well; the migrator runs against it but finds nothing to do. Seed it (below) to get a database you can click through.

`PUSpecSheetLegacy` on `(localdb)\MSSQLLocalDB` was built this way on 2026-11-17.

### Seeding an empty database

`seed.ps1` adds just enough reference data to use the app:

```powershell
./seed.ps1 -Database PUSpecSheetLegacy -AdminObjectId <your Entra object id> -AdminName '<your name>' -AdminEmail <your email>
```

| Seeded | What |
| --- | --- |
| Permissions | `admin.view`, `admin.sheets`, `admin.types`, `admin.phases`, `admin.users`, `admin.permissions`: every key the API and UI check. |
| Admin user | The user you name, with every permission, and the approver of the phase. |
| Phase | Development (`DEV`). |
| Sheet type | Engine Specification: vertical tables, approval required, in the Development phase, with the properties the admin UI gives a new type (Type, Phase, Version, Copy From) and one display level labelled by the version. |
| Cell types | All 26 the UI knows (`CellTypeReferenceKey`), with their `Type` and `ModelType`. |
| Widget types | Part pair identifier and PFK nominal (`WidgetTypeKey`). |
| Table template | Limits: a header row (Description, Min, Max, Unit) and a Limit group under it with one row of a text cell, two numeric cells (2 decimal places) and a text dropdown (mm, um, N.m, bar, degC). Both groups are default, so a new table starts with the headings and one limit; more are added from the header. |

The API finds the signed-in user by the `oid` claim of their token, so `-AdminObjectId` must be your Entra ID object id for the permissions to be yours (it's on your user's page in the Entra admin centre, or in the `oid` claim of a token from the app). Leave it out and the admin is a placeholder user (`00000000-0000-0000-0000-000000000001`); signing in then creates you a second user with no permissions. Run the script again with your object id to fix that.

It only inserts what is missing, matching each row on its natural key (a permission's key, a user's object id, a phase's or type's name, a cell or widget type's key, a template's name within its type, a group's title, a row's number and a cell's column). Running it again changes nothing, and nothing it finds is updated or deleted. It touches the reference tables only.

The `ModelType` of the dropdown and lookup cell types (text and numeric dropdowns, table and linked sheet values, engine spec part, engine phase) is read from the UI's components rather than from a legacy database. Check them against one when you can.

## Schemas

| Schema | Holds |
| --- | --- |
| `access` | Users and their permissions. |
| `types` | Phases, sheet types, their properties and structure levels, widget types. |
| `template` | Table templates, their groups, rows and cells, cell types and template widgets. Sheets of both layouts are built from these. |
| `configuration` | Cell configurations. |
| `legacy` | Sheets as copies: every version of a sheet is a whole copy of its tables, groups, rows, cells and values, with workflow steps for approval. These are the tables the revisions (`spec`, `cell`, `approval`) replace; see [The legacy schema](#the-legacy-schema). |
| `spec` | Sheets as revisions (stage 3 onwards): see below. |
| `cell` | The values of `spec` revisions: a row revision's cell values, and a table or group revision's widget values and a table revision's note values. |
| `approval` | Submissions of a sheet's drafts and their approvers. |

### The legacy schema

The 19 tables of the sheet copies are in `legacy`, because the revisions replace them: `SpecSheets`, `SpecSheetTables`, `SpecSheetTableGroups`, `SpecSheetTableRows`, `SpecSheetTableCells`, `SpecSheetTableWidgets`, `SpecSheetTableGroupWidgets`, `SpecSheetTableNotes`, `SpecSheetLinkedTables` and `SpecSheetPropertyValues` (formerly `dbo`), `TextValues`, `NumericValues`, `PairValues`, `LinkedSpecSheetValues`, `TableWidgetValues` and `TableGroupWidgetValues` (formerly `values`), `SpecSheetExternalReferences` (formerly `references`), and `WorkflowSteps` and `WorkflowStepActors` (formerly `workflow`). Users, permissions, types, phases, templates and cell configurations aren't replaced, so they stay in `access`, `types`, `template` and `configuration`.

The moves are in `PUSpecSheet.Database.refactorlog`, which both projects use. When `deploy.ps1` meets a database that still has the copies in their old schemas, SqlPackage moves them with `ALTER SCHEMA TRANSFER`, rows, keys and identity values included, rather than creating empty tables in `legacy`, and records the moves in `dbo.__RefactorLog` so it doesn't try them again. A database already using `legacy` (the legacy database itself, going by the dump) is left as it is. Run `compare.ps1` first to see what it will do. The old schemas are left behind, empty.

To move the tables by hand instead (to check the move on its own before deploying, say), run `legacy/move-to-legacy-schema.sql`. It does the same transfers in one transaction, records them in `dbo.__RefactorLog` as SqlPackage would, drops `values`, `references` and `workflow` once they are empty, and can be run again. It changes nothing if a table is in both its old schema and `legacy`:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -d PUSpecSheetLegacy -E -b -i legacy\move-to-legacy-schema.sql
```

### Sheets as revisions

A sheet (`spec.Sheets`) is one per phase and sheet type. Its tables, groups, rows, properties, linked tables and external references are each an item with a stable identity, and everything about an item that can change is in its revisions:

| Item | Revisions hold |
| --- | --- |
| `SheetTables` (a `template.TableTemplates` table) | Title, display order, configuration override |
| `SheetTableGroups` (a `template.TableGroupTemplates` group) | Display order (the copies' `Order`), the linked (parent) group's ReferenceID |
| `SheetTableRows` (a `template.TableRowTemplates` row) | Display order (the copies' `RowNumber`, by which a nested group's rows are matched with its parent's), and its cells' values in `cell.*` |
| `SheetProperties` (a `types.SpecSheetTypeProperties` property) | The value |
| `SheetLinkedTables` (another sheet's table, by ReferenceID) | Display order |
| `SheetExternalReferences` | Everything, published only: references belong to versions |

A table's widgets (`SheetTableWidgets`, one per `template.TableTemplateWidgets` widget) and notes (`SheetTableNotes`, with their writer, time and source system), and a group's widgets (`SheetTableGroupWidgets`), are items with no revisions of their own, like cells. What a revision holds of them is a row in `cell.TableWidgetValues`, `cell.TableNoteValues` or `cell.TableGroupWidgetValues`, keyed by the revision and the item: the widget's value (JSON, which may be empty), or the note's text and whether it is obsolete. A revision has a widget or note if it has its row, so a draft is started by copying the published revision's rows.

A row's cells (`SheetTableCells`, one per `template.TableCellTemplates` cell) have no revisions of their own; their values (`cell.TextValues`, `NumericValues`, `PairValues`, `LinkedSheetValues`) and any configuration override (`cell.CellConfigurationOverrides`) are keyed by the row revision and the cell.

The revision tables share one shape: `RevisionNumber`, `Status` (0 draft, 1 published), `IsDeleted`, `CreatedByUserID`, created, last modified, published and superseded date times, the `SheetVersionID` it was published in and the `SheetSubmissionID` of the approval it waits on, and a `ROWVERSION`. Check constraints tie the published and superseded times to the status, and filtered unique indexes allow one draft and one current published revision per item. A sheet as it was at any time is the published revisions whose published time is at or before it and whose superseded time is after it or empty.

Until editing moves onto the revisions, they are built from the copies by `api/tools/PUSpecSheet.Migrator`, which keeps a hash of the copies a sheet was built from in `spec.Sheets.LegacySourceHash` and rebuilds the sheet when they change.

Each publish is a `spec.SheetVersions` row, numbered from 1, with its change log. Migrated versions keep the `SpecSheets` copy they came from (`SourceSpecSheetID`), its creator and creation time (`SourceCreatedByUserID`, `SourceCreatedDateTime`) and its `LegacySpecSheetID` and `HasBeenMigrated`.

Items keep the `ReferenceID` they have in the copies. A copy for another phase keeps them too, so a ReferenceID is unique within a sheet, not across sheets; linked values and linked tables name the sheet type and phase alongside it, as the copies do.

Groups aren't nested, as copied groups aren't: nesting comes from the group templates. There are no template versions: a sheet's items point at the template tables directly, so a template change shows in every version of every sheet built from it, as it does with the copies.

## Where the scripts came from

The scripts were written from the EF model (`PUSpecSheetDbContext` and its configurations), which decides the schemas, tables and columns, with the physical detail EF doesn't hold (varchar or nvarchar, lengths, nullability, defaults, column order and foreign key names) taken from the column dump of the database in `docs/legacy-schema.txt`. CellConfigurations isn't in the dump, so it comes from the EF model alone.

Two things are conventions rather than read from anywhere: primary keys are named `PK_<table>` and defaults `DF_<table>_<column>`, and every foreign key column has an index `IX_<table>_<column>` (unique for the one-to-one value tables), as EF would create. The database's own constraint and index names will differ, and `compare.ps1` will list them the first time it is run against it.

Foreign keys don't cascade, except SpecSheetTypePhases to SpecSheetTypes, the only cascade the EF model configures. EF would make every required relationship cascade, which SQL Server rejects here (a sheet table is reachable from a sheet type by more than one path).

### Where the EF model and the database disagreed

The EF configurations have since been brought into line with the database: varchar column types, the two columns that allow NULL (`WorkflowStepActors.ActorUserID`, `TableTemplates.RequiresUserConfiguration`), the foreign keys EF didn't know about (`TableGroupTemplates.ParentTableGroupTemplateID`, `LinkedSpecSheetValues.LinkedSpecSheetTypeID` and `LinkedSpecSheetPhaseID`), and SpecSheetLinkedTables, which is now mapped with the columns it has.

The dump puts the sheet copies (SpecSheets and its tables, groups, rows, cells, widgets, notes, linked tables, property values, values, external references and workflow steps) in a schema called `legacy`. The scripts first followed the EF model, which had them in `dbo`, `values`, `references` and `workflow`; since 2026-11-17 the scripts and the EF model both use `legacy`, as the dump does.

One difference is left on purpose: EF relates `SpecSheetTableNotes.CreatedByUserID` to Users so it can load the author, but the database has no foreign key for it. EF doesn't need one, and adding it could fail on notes whose author has gone.

The dump also gives every datetime2 column a scale of 0, so the scripts use `DATETIME2 (0)`. Confirm it against the database when you can; if it's really `DATETIME2 (7)`, `compare.ps1` will flag every one.

## Checking against a database

When someone can run the scripts against a database, run `compare.ps1` and settle what it lists, either by fixing the project or, where the database is right and the project wrong, with `extract.ps1`. Until then the project is the best reconstruction available, not a verified record.

## Why not just `dotnet ef dbcontext script`

It needs the EF design-time package and tools added to the API, and it only knows the EF model, so its output differs from the database everywhere in the table above. The scripts here start from the same model and correct it from the dump.
