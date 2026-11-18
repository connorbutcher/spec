# PUSpecSheet.Migrator

Keeps the revision tables (`spec`, `cell` and `approval`, see [database/README.md](../../../database/README.md)) in step with the copied sheets (`SpecSheets` and the tables under it), which the app still writes. Run it as often as you like: each run rebuilds only the sheets whose copies have changed since it last built them.

It reads and writes through the app's own `PUSpecSheetDbContext`, in the one database.

## Running it

```powershell
# Dry run: every changed sheet is built, checked and rolled back.
dotnet run --project api/tools/PUSpecSheet.Migrator -- --connection "Server=...;Database=PUSpecSheet;..."

# Keep what is built.
dotnet run --project api/tools/PUSpecSheet.Migrator -- --connection "<connection string>" --commit
```

| Option | |
| --- | --- |
| `--connection <connection string>` | The PUSpecSheet database. Required. |
| `--phase <phase id>` | Only this phase's sheets. Can be given more than once. |
| `--rebuild` | Build every sheet again, whether its copies changed or not. |
| `--commit` | Keep what is built. Without it, it's a dry run. |

It exits with 0 when every sheet was migrated, checked or already up to date, 1 when any failed, and 2 for bad arguments.

## What a run does

A sheet is every `SpecSheets` row with one phase and sheet type. For each:

1. **Read** every copy (`Legacy/LegacySheetReader`, with `LegacySheetScope` narrowing each table to the sheet and the cell value, widget and workflow readers beside it) into a `SheetSnapshot`: what the copy shows, leaving out deleted tables, groups, rows and cells.
2. **Compare** a hash of the copies (`Legacy/LegacyFingerprint`: each copy's publish details, what it shows and its workflow) with the one `spec.Sheets.LegacySourceHash` kept when the sheet was last built. If they match, the sheet is up to date and left alone.
3. **Delete** what was built for the sheet before, if anything, and **build** it again, all in one transaction:
   - Items are followed from copy to copy by their `ReferenceID`, which a copy made from another keeps (`Building/KeyAssigner`), and keep it. An item whose ReferenceID turns up under another parent or template is treated as removed and added again, with a warning, since an item can't move or change its template.
   - Each published copy, oldest first, becomes a `SheetVersion`. Each item gets a published revision only when it is new or differs from its last one (its order included, as the copy numbers it), and that revision replaces the one before at the same moment (`Building/SheetHistoryBuilder`; `RevisionFactory` makes the revisions and `SubmissionPlanner` the submissions).
   - An item missing from a copy gets a removing revision, unless the table or group it was in went too.
   - The working copy, if there is one, becomes drafts held by its creator.
   - Workflow steps become submissions, saved after the versions they published, and then their revisions are pointed at them.
4. **Check** it (`Verification/`): the sheet's history is read back once (`TargetSheetHistory`) and shown as of every version, and with its drafts, and each is compared with its copy, rendered as text in order (tables, groups, rows and cells with their orders, ReferenceIDs and contents, widgets, notes and configuration overrides, properties, linked tables, and for versions the external references). The first difference is logged.
5. **Commit** only if every check matched and `--commit` was given. Otherwise it rolls back, and the sheet as it was before the run is untouched.

A sheet in `spec.Sheets` without a `LegacySourceHash` wasn't built by the migrator, so it is never replaced.

The rebuilt sheet keeps its `ReferenceID`; the ids of everything under it change with every rebuild, so nothing should hold on to them until the copies stop being written.

## How the copies map

| Copies | Revisions |
| --- | --- |
| `SpecSheets` rows with one phase and sheet type | One sheet. Each published copy (`IsPublished = 1`) is a version, numbered oldest first, published at `PublishedDateTime` by `PublishedByUserID`. The copy without it is the working copy. Versions published at the same moment are moved a tick apart, with a warning. |
| `ChangeLog` | The version's change log, as it is. |
| `SpecSheetID`, `LegacySpecSheetID`, `HasBeenMigrated`, `CreatedByUserID`, `CreatedDateTime` | Kept on the version as `SourceSpecSheetID`, `LegacySpecSheetID`, `HasBeenMigrated`, `SourceCreatedByUserID` and `SourceCreatedDateTime`. |
| `SpecSheetTables` | Sheet tables, on the same table template. `DisplayOrder`, the title and `ConfigurationOverride` are kept on the revision. |
| `SpecSheetTableGroups` | Sheet table groups, on the same group template, with their `Order` as the revision's display order. The linked group (the parent the app nests the group under) is kept as its ReferenceID. |
| `SpecSheetTableRows` | Sheet table rows, on the same row template, with their `RowNumber` as the revision's display order. The app matches a nested group's rows with its parent's by this number, so it is kept exactly, not just as a place. |
| `SpecSheetTableCells` | Sheet table cells. A copied cell doesn't name its template cell, so it is the one at the same `ColumnNumber` in its row's template. Its `ConfigurationOverride` is kept with its values; a cell type, configuration or `RowNumber` of its own, other than its template cell's or its row's, isn't, and the warnings count them. |
| `TextValues`, `NumericValues`, `PairValues`, `LinkedSpecSheetValues` | `cell.TextValues`, `NumericValues`, `PairValues` and `LinkedSheetValues`, on the row revision. Where a cell has more than one value of a kind, the newest is kept, with a warning. |
| `SpecSheetTableWidgets` / `TableWidgetValues`, and the group equivalents | The table or group revision's `WidgetValues`, by template widget. |
| `SpecSheetTableNotes` | The table revision's `Notes`. |
| `SpecSheetPropertyValues` | Sheet properties, one per sheet type property, whose revisions hold the value. |
| `SpecSheetExternalReferences` | External references, followed by `ReferenceID`, with a revision for each version where one is new, changed or gone. A working copy's references aren't kept, as references only belong to versions. |
| `SpecSheetLinkedTables` | Linked tables, followed by the linked table's ReferenceID, with their `DisplayOrder`. |
| `WorkflowSteps` / `WorkflowStepActors` | Submissions and their approvers, as below. |

How workflow steps become submissions:

- **Approving step on a published copy:** the last approving step is the submission that published the version, and the revisions written for the version are its changes.
- **Pending step on the working copy:** the working copy's last step, if pending, is a submission awaiting approval, holding the drafts.
- **Other rejected or approved steps:** kept as decided.
- **Pending steps a later step or a publish overtook:** kept as withdrawn.
- **Approvers:** an actor's comment is the reason. A rejection without one is given "(no reason given)".

`Legacy/LegacyWorkflowStates` maps the `State` numbers by the app's `WorkflowState`. A state it doesn't know stops the sheet rather than guessing.

## Changing it

The hash includes `LegacyFingerprint.BuildVersion`. Change it whenever a change here would build a sheet differently from the same copies, so that the next run rebuilds every sheet.

## Tests

`api/tests/PUSpecSheet.Migrator.Tests` covers the history building, which needs no database.
