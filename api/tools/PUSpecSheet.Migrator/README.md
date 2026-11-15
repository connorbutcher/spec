# PUSpecSheet.Migrator

A one-off console tool that moves sheets from the flat layout, where every version of a sheet is a whole copy of it, onto the revision layout described in [database/README.md](../../../database/README.md). It reuses the app's `Db*` models and `PuSpecSheetDbContext` to write, and the app's own "as of" query (`SheetRevisionQueryExtensions.InView`) to check what it wrote.

## Running it

```powershell
# 1. Set the flat tables aside, then deploy the new schema beside them.
sqlcmd -S "(localdb)\MSSQLLocalDB" -d PUSpecSheet -b -i database/OneTime/MoveFlatTablesToLegacy.sql
./database/deploy.ps1

# 2. Dry run: every sheet is migrated, checked and rolled back. Nothing is kept.
dotnet run --project api/tools/PUSpecSheet.Migrator -- --legacy "Server=(localdb)\MSSQLLocalDB;Database=PUSpecSheet;Trusted_Connection=True;TrustServerCertificate=True"

# 3. The real thing.
dotnet run --project api/tools/PUSpecSheet.Migrator -- --legacy "<connection string>" --commit
```

| Option | |
| --- | --- |
| `--legacy <connection string>` | The database holding the `[legacy]` tables. Required. |
| `--target <connection string>` | The database to migrate into. Defaults to `--legacy`. Legacy template, phase, sheet type, cell type and user ids are used as they are, so the target's design and setup tables must have the same ids. |
| `--phase <phase id>` | Only this phase's sheets. Can be given more than once. |
| `--commit` | Keep what is migrated. Without it, it's a dry run. |

It exits with 0 when every sheet was migrated or checked, 1 when any failed and 2 for bad arguments. A sheet whose public id is already in the target is skipped, so after fixing a failure you can run it again and it carries on where it stopped.

## What it does with each sheet

Each sheet is done in its own transaction:

1. **Read** every copy of the sheet (`Legacy/LegacySheetReader`) into a `SheetSnapshot`.
2. **Match** the items in each copy with the same items in the copy before (`Building/KeyAssigner`, `Building/SiblingMatcher`). An item is only matched with one from the same template under the same matched parent. Rows are matched on their contents, like lines in a text diff, so a row inserted in the middle doesn't make every row after it look changed. Tables are matched on their titles, and sections and column blocks on their order.
3. **Build** the history (`Building/SheetHistoryBuilder`). Each published copy becomes a `SheetVersion`. Each item gets a published revision only when it is new or differs from its last one. Each new revision replaces the one before at the same moment. An item missing from a copy gets a removing revision, unless the table or section it was in went too. The unpublished working copy, if there is one, becomes drafts held by its editor.
4. **Save** it all as inserts.
5. **Check** it (`Verification/`). The sheet is read back as of every version, and with its drafts, and each is compared with its legacy copy. The comparison renders both as text listing tables, sections, rows and cell contents in display order, with no ids. The first difference is logged.
6. **Commit** only if every check matched and `--commit` was given. Otherwise it rolls back.

Approvals aren't migrated. Migrated versions have no `SheetSubmission`, and their revisions have no `SheetSubmissionId`. This is the same as the versions start-up seeding publishes, and the app shows them as published without an approval record.

Version numbers are kept. If two versions have the same publish time, or one is older than the version before it, the later one is moved to one tick after the earlier, and a warning is logged. Otherwise "as of" a time couldn't tell them apart.

## The legacy layout

The only code that knows the legacy tables is the SQL in `Legacy/LegacySql.cs`. If a name there is wrong, change it there. It reads these tables from the `[legacy]` schema:

| Table | Columns read |
| --- | --- |
| `PUSpecSheet` | `Id`, `PublicId`, `PhaseId`, `SheetTypeId`, `Version`, `Published`, `PublishedAt`, `PublishedById`, `CreatedById` |
| `Table` | `Id`, `SpecSheetId`, `TableTemplateId`, `Title` |
| `TableGroup` | `Id`, `TableId`, `ParentGroupId`, `GroupTemplateId` |
| `TableRow` | `Id`, `GroupId`, `RowTemplateId` |
| `Cell` | `Id`, `TableRowId`, `TemplateCellId` |
| `TextValues`, `NumericValues`, `DateValues`, `BooleanValues` | `CellId`, `Value` |
| `OptionValues` | `CellId`, `CellTypeOptionId` |
| `LabelledTextValues` | `CellId`, `SlotKey`, `Position`, `Value` |
| `CellSettings` | `CellId`, `Settings` (JSON, as `cell.CellSettings` stores it) |

How it maps onto the new layout:

- **Sheets:** all `PUSpecSheet` rows with the same phase and sheet type are versions of one sheet. The migrated sheet keeps the latest version's public id.
- **Published or working copy:** a row with `Published = 1` becomes a version, published at `PublishedAt` (taken as UTC) by `PublishedById`. A row without it is the working copy, which becomes drafts held by `CreatedById`. A sheet can have only one working copy.
- **Templates:** a table becomes a copy of the latest version of its `TableTemplateId`. Groups become sections, and `GroupTemplateId`, `RowTemplateId` and `TemplateCellId` are used as the new layout's template ids.
- **Order:** legacy items have no order of their own. Among one parent's items, they're placed by their template's display order, then by id. The template orders come from the target's design tables. Like the app, an item keeps the order it was published with while that still fits, and a new item goes between its neighbours (`Building/OrderKeeper`). That way an inserted row doesn't give every row below it a new order and a new revision.
- **Ids:** legacy tables, groups, rows and cells have no public ids, so they get new ones. The legacy layout has no column blocks, so none are made.

## Tests

`tests/PUSpecSheet.Migrator.Tests` covers the matching and the history building, which need no database.
