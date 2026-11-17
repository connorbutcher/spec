# PUSpecSheet.Migrator

A one-off console tool that moves sheets from the legacy app, where every version of a sheet is a whole copy of it, onto the revision layout described in [database/README.md](../../../database/README.md). The legacy tables are listed in [api/legacy.txt](../../legacy.txt). The tool reuses the app's `Db*` models and `PuSpecSheetDbContext` to write, and the app's own "as of" query (`SheetRevisionQueryExtensions.InView`) to check what it wrote.

## Before running it

- **Deploy the new schema** (`./database/deploy.ps1`) to the database you're migrating into. That can be the legacy database itself: the new tables are in other schemas.
- **Set up the target by hand, with the legacy ids.** This covers phases, sheet types, users, cell types and templates. Each legacy template table maps to the target table that uses the same ids:

  | Legacy table | Target table with the same ids |
  | --- | --- |
  | `TableGroupTemplates` | template sections |
  | `TableRowTemplates` | template rows |
  | `TableCellTemplates` | template cells |

  A legacy table template becomes the template version that holds its group templates.
- **Give pair cell types two slots.** A pair cell type must be a labelled text cell type in the target, with slots `x` and `y` (`LegacySheetReader.PairSlotX` and `PairSlotY`).
- **Check the workflow states.** `Legacy/LegacyWorkflowStates` says what the `State` numbers of `WorkflowSteps` and `WorkflowStepActors` mean. It assumes 0 = waiting, 1 = approved, 2 = rejected. Change it there if the legacy app numbered them differently. A state it doesn't know stops the sheet rather than guessing.

## Running it

```powershell
# Dry run: every sheet is migrated, checked and rolled back.
dotnet run --project api/tools/PUSpecSheet.Migrator -- --legacy "Server=(localdb)\MSSQLLocalDB;Database=PUSpecSheet;Trusted_Connection=True;TrustServerCertificate=True"

# The real thing.
dotnet run --project api/tools/PUSpecSheet.Migrator -- --legacy "<connection string>" --commit
```

| Option | |
| --- | --- |
| `--legacy <connection string>` | The database holding the legacy tables. Required. |
| `--target <connection string>` | The database to migrate into. Defaults to `--legacy`. |
| `--phase <phase id>` | Only this phase's sheets. Can be given more than once. |
| `--commit` | Keep the migrated sheets. Without it, it's a dry run. |

It exits with 0 when every sheet was migrated or checked, 1 when any failed or reference data was missing, and 2 for bad arguments. A sheet whose phase and sheet type are already in the target is skipped. After fixing a failure you can run it again and it carries on where it stopped.

## First, the reference data

`Legacy/ReferenceDataMigrator` runs once before any sheet:

- **Checks every legacy user is in the target with the same id.** If one isn't, it lists them and stops.
- **Lists what each user could do in the legacy app:** their permissions, table permissions, and the phases they observed or approved. Each sheet type's required permissions are listed too. Roles in the new app are given by hand from this list.
- **Adds the setup rows the new layout created for legacy data,** keeping their legacy ids: widget types, table and group template widgets, and sheet type properties. Rows already there are left alone. These rows are kept even on a dry run, as the sheets' checks need them.

## Then each sheet

Each sheet is done in its own transaction:

1. **Read** every copy of the sheet (`Legacy/LegacySheetReader`) into a `SheetSnapshot`.
2. **Match** the items in each copy with the items in the copies before. A copy made from another keeps each table's, group's, row's and cell's `ReferenceID`, so that is what an item is followed by (`Building/KeyAssigner`). The `ReferenceID` becomes the item's public id. An item whose `ReferenceID` turns up under another parent or template is treated as removed and added again, with a warning, since the new layout can't move an item or change its template.
3. **Build** the history (`Building/SheetHistoryBuilder`):
   - Each published copy, oldest first, becomes a `SheetVersion`.
   - Each item gets a published revision only when it is new or differs from its last one, and that revision replaces the one before at the same moment.
   - An item missing from a copy gets a removing revision, unless the table or section it was in went too.
   - The unpublished working copy, if there is one, becomes drafts held by its creator.
4. **Save** it all as inserts, then the submissions, then point each submission's revisions at it.
5. **Check** it (`Verification/`). The sheet is read back as of every version, and with its drafts, and each is compared with its legacy copy. The comparison renders both as text, in display order:
   - tables, sections, rows and cells, with their public ids and contents
   - widgets and notes
   - properties and linked tables
   - for versions, the external references

   The first difference is logged.
6. **Commit** only if every check matched and `--commit` was given. Otherwise it rolls back.

## How the legacy data maps

| Legacy | New |
| --- | --- |
| `SpecSheets` rows with one phase and sheet type | One sheet. Each published copy (`IsPublished = 1`) is a version, numbered oldest first, published at `PublishedDateTime` (taken as UTC) by `PublishedByUserID`. The copy without it is the working copy. |
| `ChangeLog` | The version's note. |
| `SpecSheetID`, `LegacySpecSheetID`, `HasBeenMigrated` | Kept on the version as `SourceSpecSheetId`, `LegacySpecSheetId` and `HasBeenMigrated`. |
| `SpecSheetTables` | Sheet tables. `DisplayOrder` gives the order, and the table template gives the template version. |
| `SpecSheetTableGroups` | Sheet sections, ordered by `Order`. Legacy groups aren't nested but their templates are. A group whose template has a parent goes in the nearest group before it, in the same table, of that parent template. If there isn't one, it goes in the first such group. A linked group is kept as `LinkedSectionPublicId`. |
| `SpecSheetTableRows` | Sheet rows, ordered by `RowNumber`. |
| `SpecSheetTableCells` | Sheet cells. A legacy cell doesn't name its template cell, so it is the one at the same `ColumnNumber` in its row's template. A cell type, configuration or `ConfigurationOverride` of the cell's own isn't kept; the warnings count them. |
| `TextValues`, `NumericValues` | Text and number values. Text over 4,000 characters stops the sheet. |
| `PairValues` | Labelled text in slots `x` and `y`. |
| `LinkedSpecSheetValues` | `cell.LinkedSheetValues`, pointing at the other sheet's table and item by public id. |
| `SpecSheetTableWidgets` / `TableWidgetValues`, and the group equivalents | The table or section revision's `WidgetValues`: JSON, by template widget id. |
| `SpecSheetTableNotes` | The table revision's `Notes`: a JSON array of `SheetTableNote`. |
| `SpecSheetPropertyValues` | Sheet properties, one per sheet type property, whose revisions hold the value. |
| `SpecSheetExternalReferences` | External references, followed by `ReferenceID`, with a revision for each version where one is new, changed or gone. A working copy's references aren't kept, as references only belong to versions. |
| `SpecSheetLinkedTables` | Linked tables, followed by the linked table's `ReferenceID` and ordered by `DisplayOrder`. |
| `WorkflowSteps` / `WorkflowStepActors` | Submissions and their approvers, as described below. |

How workflow steps become submissions:

- **Approving step on a published copy:** the last approving step is the submission that published the version, and the revisions written for the version are its changes.
- **Pending step on the working copy:** the working copy's last step, if pending, is a submission awaiting approval, holding the drafts.
- **Other rejected or approved steps:** kept as decided.
- **Pending steps a later step or a publish overtook:** kept as withdrawn.
- **Approvers:** an actor's comment is the reason, cut to 1,000 characters. A rejection without one is given "(no reason given)".

## Tests

`tests/PUSpecSheet.Migrator.Tests` covers the history building, which needs no database.
