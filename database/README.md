# PUSpecSheet.Database

The database's schema, kept as a SQL database project (Microsoft.Build.Sql): one script per table, with its keys, constraints and indexes, under `<schema>/Tables/`, and one per schema under `Security/`. Building it checks every object and reference and produces a dacpac, which SqlPackage compares with or deploys to a database.

The API reads and writes the database through EF Core (`api/src/PUSpecSheet.Api.Database`) but doesn't own the schema: there are no EF migrations. This project is the record of what the schema is. Change it here, deploy it, then update the EF schema classes and configurations to match.

## Scripts

All three restore SqlPackage as a local dotnet tool and take `-Server` and `-Database` (Windows authentication, defaulting to `(localdb)\MSSQLLocalDB` and `PUSpecSheet`) or `-ConnectionString` for anything else.

| Script | What it does |
| --- | --- |
| `extract.ps1` | Writes a database's schema into the project, replacing the schema folders. Review the result with git before committing it. |
| `compare.ps1` | Builds the project and lists what deploying it would change in a database, objects only in the database included. Changes nothing. Exits with 1 when there are differences. |
| `deploy.ps1` | Builds the project and makes a database match it, creating the database if needed. Stops instead of losing data. |

Database options, users, logins and permissions are kept out of the project and left alone by all three, since they differ between environments.

## Schemas

| Schema | Holds |
| --- | --- |
| `access` | Users and their permissions. |
| `types` | Phases, sheet types, their properties and structure levels, widget types. |
| `template` | Table templates, their groups, rows and cells, cell types and template widgets. Sheets of both layouts are built from these. |
| `configuration` | Cell configurations. |
| `dbo`, `values`, `references`, `workflow` | Sheets as copies: every version of a sheet is a whole copy of its tables, groups, rows, cells and values, with workflow steps for approval. |
| `spec` | Sheets as revisions (stage 3 onwards): see below. |
| `cell` | The cell values of `spec` row revisions. |
| `approval` | Submissions of a sheet's drafts and their approvers. |

### Sheets as revisions

A sheet (`spec.Sheets`) is one per phase and sheet type. Its tables, groups, rows, properties, linked tables and external references are each an item with a stable identity, and everything about an item that can change is in its revisions:

| Item | Revisions hold |
| --- | --- |
| `SheetTables` (a `template.TableTemplates` table) | Title, display order, configuration override, widget values and notes (JSON) |
| `SheetTableGroups` (a `template.TableGroupTemplates` group) | Display order (the copies' `Order`), the linked (parent) group's ReferenceID, widget values (JSON) |
| `SheetTableRows` (a `template.TableRowTemplates` row) | Display order (the copies' `RowNumber`, by which a nested group's rows are matched with its parent's), and its cells' values in `cell.*` |
| `SheetProperties` (a `types.SpecSheetTypeProperties` property) | The value |
| `SheetLinkedTables` (another sheet's table, by ReferenceID) | Display order |
| `SheetExternalReferences` | Everything, published only: references belong to versions |

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

One difference is left on purpose: EF relates `SpecSheetTableNotes.CreatedByUserID` to Users so it can load the author, but the database has no foreign key for it. EF doesn't need one, and adding it could fail on notes whose author has gone.

The dump also gives every datetime2 column a scale of 0, so the scripts use `DATETIME2 (0)`. Confirm it against the database when you can; if it's really `DATETIME2 (7)`, `compare.ps1` will flag every one.

## Checking against a database

When someone can run the scripts against a database, run `compare.ps1` and settle what it lists, either by fixing the project or, where the database is right and the project wrong, with `extract.ps1`. Until then the project is the best reconstruction available, not a verified record.

## Why not just `dotnet ef dbcontext script`

It needs the EF design-time package and tools added to the API, and it only knows the EF model, so its output differs from the database everywhere in the table above. The scripts here start from the same model and correct it from the dump.
