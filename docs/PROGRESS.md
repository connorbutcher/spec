# PU Spec Sheet: progress and stages

Handoff note for continuing this work in a new thread or Project.
Repo: `connorbutcher/spec` (.NET API in `api/`, Angular UI in `app/`). Last commit at time of writing: `2cd0f95`.

## Goal

The legacy codebase is the base. The rewrite's features (revision model, drafts, versions, approvals, migration) are built onto it in 8 small stages. The rewrite itself is preserved at commit `2c82183`.

## Standing preferences

- Commit straight to main. No feature branches or PRs.
- Changes to the legacy base match the existing code's style.
- Keep the existing template tables. The rewrite's design schema, template versioning, column blocks and cell kinds are not brought across.

## Stages

| # | Stage | Status | Commits |
|---|-------|--------|---------|
| 1 | Make legacy the base | Done | `7de9245` |
| 2 | SQL database project | Done | `acd53e5`, `72dea9b`, `1bde7bb` |
| 3 | Revision tables beside the copy tables | Done | `7b2ddcf` |
| 4 | Migrator (copies to revisions), re-runnable | Done | `1057cda`, `4f58572` |
| 5 | Reads from revisions behind a switch | Done | `03a8f75`, `8eecb3b`, `2cd0f95` |
| 6 | Per-sheet-type cutover of editing onto drafts and revisions | Slices 1 to 6 done; scratch databases only until checked end to end | `5cf6c7d`, `897e675`, `3b1f744`, `2ca60b3`, `497f721` |
| 7 | Port remaining features (published API, lookups, presence). Templates stay as they are | Not started | |
| 8 | Retire the legacy copy code | Not started | |

## What exists now

**Legacy model ("copies").** Every published version of a sheet is a whole copy (a SpecSheets row plus tables, groups, rows, cells, values). Schema: legacy (moved from dbo, values, references and workflow on 2026-11-17; see database/README.md).

**Revision model (beside the copies).**
- `spec`: Sheets (one per phase and sheet type), SheetVersions, and items (SheetTables, SheetTableGroups, SheetTableRows, SheetTableCells, SheetProperties, SheetLinkedTables, SheetExternalReferences), each with revisions.
- `cell`: TextValues, NumericValues, PairValues, LinkedSheetValues, CellConfigurationOverrides, keyed by (SheetTableRowRevisionID, SheetTableCellID).
- `approval`: SheetSubmissions, SheetSubmissionApprovers.
- A revision has Status (0 draft, 1 published), IsDeleted, published and superseded times, SheetVersionID, SheetSubmissionID, RowVersion. Check constraints tie the times to the status. Filtered unique indexes allow one draft and one current published revision per item.
- A sheet at a moment = published revisions where published <= moment and (superseded is null or superseded > moment).
- ReferenceID is only unique within a sheet (copies for other phases keep it).
- Order values are data: table DisplayOrder, group Order and row RowNumber are kept exactly as the copies have them. A row's RowNumber is how the query nests a group's rows under its parent's. A group's LinkedSpecSheetTableGroupId is the parent group.

**Database project** (`database/`, Microsoft.Build.Sql 2.3.0): scripts per `<schema>/Tables/<Table>.sql`, plus `extract.ps1`, `compare.ps1`, `deploy.ps1` and a README. Generated from the EF model plus `docs/legacy-schema.txt`, not from a live database.

**EF** (`api/src/PUSpecSheet.Api.Database`): `Schema/{Spec,Cell,Approval}` records, `Configuration/{Spec,Cell,Approval}` configurations. `DbSheet` has `LegacySourceHash` and `MigratedDateTime`. `DbSheetVersion` has the Source columns.

**Migrator** (`api/tools/PUSpecSheet.Migrator`, tests in `api/tests/PUSpecSheet.Migrator.Tests`):
- Reads copies, fingerprints them (`LegacyFingerprint`, BuildVersion "2"), rebuilds only changed sheets, one transaction per sheet.
- Verifies by reading each sheet back as of every version (plus drafts) and comparing to each copy. Commits only with `--commit`.
- Never replaces sheets without a LegacySourceHash (those belong to the new app).
- Workflow steps become submissions. Cells map to their template cell by ColumnNumber.
- Usage: `dotnet run --project api/tools/PUSpecSheet.Migrator -- --connection "..." [--phase N] [--rebuild] [--commit]`

**Reads** (`api/src/PUSpecSheet.Api.DAL/DataAccess/SpecSheets/Reads/`):
- `SheetReads:Source` = Copies | Compare | Revisions (appsettings, default Copies). Used by `GetSpecSheetByIdAsync` on a cache miss.
- `SpecSheetRevisionReader` builds the same DbSpecSheet graph via one filtered-include split query, then uses the existing `MapToDomain`.
- `GET /revisions/compare` (AdminController) lists differences and sheets not yet migrated.
- Compare mode doubles read cost on a cache miss.

## Stage 6: editing on drafts and revisions

**The switch.** `SheetEdits:RevisionSheetTypeIds` in appsettings, a list of sheet type ids, empty by default. For a listed type:
- A draft (an unpublished SpecSheets row) is read from the revisions: each item's draft where it has one, otherwise its current published revision. Tables, groups, rows and cells carry their spec items' ids, and a value's id is its cell's, so the edits the screens send can be applied to the revisions as they are.
- Edits are applied to the drafts (`Processors/Drafts`). A row's first edit starts its draft from its published revision, values included.
- The SpecSheets row stays as the draft's header: its id, creator and place in the lists.
- The first edit clears the sheet's `LegacySourceHash`, so the migrator leaves it alone from then on.
- Published sheets of a listed type are read with `SheetReads:Source` = `Revisions`.

**Before a type is listed:** the migrator has run with `--commit` and `/revisions/compare?specSheetTypeId=` is clean for it, and the scheduled migrator run is stopped for it.

**Slices.** Each is a commit or two. Until slice 5 is done the switch is for scratch databases only: whatever a slice doesn't cover yet is refused for a listed type.

| # | Slice | Status |
|---|-------|--------|
| 1 | The switch, the draft read from the revisions, and cell value edits (add and update text, number, pair and linked values on existing cells). Creating, publishing and deleting a listed type's sheets, and every other edit, are refused. | Done |
| 2 | Rows and cells: add, reorder, delete, and values and configuration overrides on new cells. A cell is kept as its row template's cell in the same column, so a cell can't be moved to another column, and deleting a cell clears what the draft holds for it (it goes with its row). | Done |
| 3 | Groups and tables, with their widgets, notes and configuration overrides. Every edit the screens send is now applied to the drafts. Widgets and notes are kept inside their table's or group's revision, so the draft read numbers them within the sheet in the order of their tables' and groups' ids, and `DraftItemNumbers` finds them again from those numbers. Deleting a group removes the groups linked to it. | Done |
| 4 | Create a draft (empty, or copied from another sheet) with its properties; delete a draft (drop its draft revisions and what was added in it). `SheetDraftCreator` makes the sheet (if it has none) and the header, `SheetDraftCopier` makes the drafts show the sheet copied from, matching items by ReferenceID and changing only what differs, and `SheetPropertyDrafts` sets the property values. `SheetDraftDeleter` drops the drafts when the header is deleted; a draft awaiting approval, or a published version, can't be deleted. | Done |
| 5 | Publish: without approval (the drafts become a version), and with approval (submission, approvers, emails, approve, reject, reassign). `SheetDraftPublisher` makes the next version, replaces each item's published revision with its draft at the version's moment (whole seconds) and marks the header published. The workflow steps on the header still drive the screens, so their ids don't change; `SheetSubmissions` keeps a submission in step with them (submitted with its approvers, each decision, reassignments) and tags the drafts while it is pending. Approval publishes the drafts and names the version on the submission; rejection hands the drafts back. A published version of a listed type is always read from the revisions, whatever `SheetReads:Source` says. | Done |
| 6 | Version lists and property summaries for listed types. The lists already read the SpecSheets headers, which listed types keep; a new draft's property values are written to its header too (they never change afterwards). The analytics dataset's cells come from the whole sheet read for a listed type. | Done |

**Order of sheet types.** First a test type made on a scratch copy. Then the types without an approval workflow, fewest sheets first. Then the approval types, once slice 5 has run on one of them. The busiest types last. Linked cells and tables read other sheets through `GetSpecSheetByIdAsync`, so a type can move before or after the types it links to.

**Verifying a slice.** On two scratch databases restored from one backup, make the same edits to the same draft, one with the type unlisted (copies) and one listed (revisions), and compare the two drafts' reads with `SpecSheetReadComparer`, which matches items by ReferenceID and ignores ids. Then build and run the tests.

**Slice 1 was checked** on a scratch database (one published copy and a working copy, migrated): the draft read shows the drafts over the published revisions; value edits in one call start a row's draft once and copy its values; text cleared removes the value row; a pair edit keeps the other half; the published read is unchanged; a batch with an unsupported operation changes nothing; the first edit clears the hash and the migrator then leaves the sheet alone.

**Slice 2 was checked** the same way: a row added with its cells, values and an override in one call; a row reordered; a row deleted with its cells; an override set and cleared; and refusals for moving a cell's column, editing a removed row, a cell with no template cell and a group on another sheet. The published read was unchanged.

**Slice 3 was checked** the same way: a table added with a group, a linked group, a row, a cell and a value in one call, plus a table widget and a group widget; a table renamed and moved; notes added and one made obsolete; widgets on two tables and a group changed by their numbers; a parent group deleted with its linked group; a table deleted; and refusals for a widget on a removed table and a widget number past the end. The published read was unchanged.

**Slice 4 was checked** the same way: a draft for a new phase copied from a published sheet (a new sheet with every item added); a second draft refused; that draft deleted (only the empty sheet is left); a draft copied from its own published version (only the properties change); a draft copied from another phase's edited draft (one row changed, one added); and a draft with nothing to copy (the table removed in the draft). The published read was unchanged throughout.

**Slice 5 was checked** on a scratch database by driving the publisher and submissions directly: a draft published without approval became version 2, with version 1 still reading as it was; a draft submitted, approved and published became version 3 with the submission naming it; a fourth draft submitted and rejected had its drafts handed back and could then be deleted. The wiring in SpecSheetDataAccessService (publish, approve, reject, reassign) was built and reviewed but not run, as the service needs Redis and the email services.

**Known limits.** Two versions of one sheet published in the same second would share a moment. A cell added to a row that is already published shows, empty, in the published read too, as the revisions don't keep cells per revision (deleting the draft removes it again). External references aren't copied into a new draft: the revisions keep them per version, pushed by the external systems, so they're for stage 7. A copied table keeps its notes' authors and times, where a new copy made them the copier's. `UpdateSpecSheetAsync` logs and swallows errors, as it always has, so a refused edit only shows in the log. The draft's workflow steps are still the header's.

## Open items

1. Nothing has been compiled or tested (no dotnet SDK in the sandbox). First step on a real machine: build the solution and run `dotnet test`.
2. Run the migrator as a dry run, then with `--commit`, against a copy of production.
3. Call `/revisions/compare` until it is clean, then try `Compare` and `Revisions` modes.
4. Check the datetime2 scale (the dump shows 0; scripts use `DATETIME2 (0)`; unverified).
5. Resolved 2026-11-17: the sheet copies' tables are in the `legacy` schema in the scripts and the EF context, as in `docs/legacy-schema.txt`. A refactor log moves them on deploy.
6. Push the reference tag yourself: `git tag new-app-reference 2c82183 && git push origin new-app-reference`.
7. Known limits: version lists, property summaries and the analytics dataset still read the copies. Empty cell values are synthesized from the cell type's ModelType. A version's workflow steps are reconstructed from submissions since the previous version. `/revisions/compare` has no authorization attribute (like the existing cache endpoints). Ids under a sheet change on every rebuild; the sheet keeps its ReferenceID.
8. A deliberate EF leftover: SpecSheetTableNotes.CreatedByUserId relates to Users in EF but the DB has no FK.
