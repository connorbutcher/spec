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
| `--target <connection string>` | The database to migrate into. Defaults to `--legacy`. A separate target must already have the same phases, sheet types, templates, cell types and users, with the same ids. |
| `--sheet <legacy sheet id>` | Only this sheet. Can be given more than once. |
| `--commit` | Keep what is migrated. Without it, it's a dry run. |

It exits with 0 when every sheet was migrated or checked, 1 when any failed and 2 for bad arguments. A sheet whose public id is already in the target is skipped, so after fixing a failure you can run it again and it carries on where it stopped.

## What it does with each sheet

Each sheet is done in its own transaction:

1. **Read** every copy of the sheet (`Legacy/LegacySheetReader`) into a `SheetSnapshot`.
2. **Match** the items in each copy with the same items in the copy before (`Building/KeyAssigner`, `Building/SiblingMatcher`). An item is only matched with one from the same template under the same matched parent. Rows are matched on their contents, like lines in a text diff, so a row inserted in the middle doesn't make every row after it look changed. Tables are matched on their titles, and sections and column blocks on their order.
3. **Build** the history (`Building/SheetHistoryBuilder`). Each published copy becomes a `SheetVersion`. Each item gets a published revision only when it is new or differs from its last one. Each new revision replaces the one before at the same moment. An item missing from a copy gets a removing revision, unless the table or section it was in went too. The working copy (the one with no `PublishedAtUtc`), if there is one, becomes drafts held by its editor. Items keep the public ids they had in their latest copy, so links into the current sheet keep working.
4. **Save** it all as inserts.
5. **Check** it (`Verification/`). The sheet is read back as of every version, and with its drafts, and each is compared with its legacy copy. The comparison renders both as text listing tables, sections, rows and cell contents in display order, with no ids. The first difference is logged.
6. **Commit** only if every check matched and `--commit` was given. Otherwise it rolls back.

Version numbers are kept. If two versions have the same publish time, or one is older than the version before it, the later one is moved to one tick after the earlier, and a warning is logged. Otherwise "as of" a time couldn't tell them apart.

## The legacy layout

The only code that knows the legacy tables is the SQL in `Legacy/LegacySql.cs`. If the real tables differ from what it assumes, change it there. It assumes these tables, in the `[legacy]` schema:

| Table | Columns read |
| --- | --- |
| `Sheets` | `Id`, `PublicId`, `PhaseId`, `SheetTypeId`, `CreatedAtUtc` |
| `SheetVersions` | `Id`, `SheetId`, `VersionNumber`, `PublishedAtUtc` (null for the working copy), `PublishedByUserId`, `Note` |
| `SheetTables` | `Id`, `PublicId`, `SheetVersionId`, `TableTemplateVersionId`, `Title`, `DisplayOrder` |
| `SheetSections` | `Id`, `PublicId`, `SheetTableId`, `ParentSheetSectionId`, `TemplateSectionId`, `DisplayOrder` |
| `SheetColumnBlocks` | `Id`, `PublicId`, `SheetTableId`, `TemplateColumnBlockId`, `DisplayOrder` |
| `SheetRows` | `Id`, `PublicId`, `SheetSectionId`, `TemplateRowId`, `DisplayOrder` |
| `SheetCells` | `Id`, `PublicId`, `SheetRowId`, `TemplateCellId`, `SheetColumnBlockId` |
| `TextValues`, `NumericValues`, `DateValues`, `BooleanValues` | `SheetCellId`, `Value` |
| `OptionValues` | `SheetCellId`, `CellTypeOptionId` |
| `LabelledTextValues` | `SheetCellId`, `SlotKey`, `Position`, `Value` |
| `CellSettings` | `SheetCellId`, `Settings` (JSON, as `cell.CellSettings` stores it) |

## Tests

`tests/PUSpecSheet.Migrator.Tests` covers the matching and the history building, which need no database.
