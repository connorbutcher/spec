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
| 6 | Per-sheet-type cutover of editing onto drafts and revisions | **Next, not started** | |
| 7 | Port remaining features (published API, lookups, presence). Templates stay as they are | Not started | |
| 8 | Retire the legacy copy code | Not started | |

## What exists now

**Legacy model ("copies").** Every published version of a sheet is a whole copy (a SpecSheets row plus tables, groups, rows, cells, values). Schemas: dbo, values, references, workflow.

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

## Next: stage 6

Move editing onto drafts and revisions one sheet type at a time. Start by reading how editing, publishing and copying work today (copier, publisher, cell filler data services), then design draft create/edit/publish against the revision tables, switching per sheet type behind a setting like the read switch.

## Open items

1. Nothing has been compiled or tested (no dotnet SDK in the sandbox). First step on a real machine: build the solution and run `dotnet test`.
2. Run the migrator as a dry run, then with `--commit`, against a copy of production.
3. Call `/revisions/compare` until it is clean, then try `Compare` and `Revisions` modes.
4. Check the datetime2 scale (the dump shows 0; scripts use `DATETIME2 (0)`; unverified).
5. Resolve schema names: the EF context maps dbo/values/references/workflow, but `docs/legacy-schema.txt` and the old migrator SQL use a `legacy` schema.
6. Push the reference tag yourself: `git tag new-app-reference 2c82183 && git push origin new-app-reference`.
7. Known limits: version lists, property summaries and the analytics dataset still read the copies. Empty cell values are synthesized from the cell type's ModelType. A version's workflow steps are reconstructed from submissions since the previous version. `/revisions/compare` has no authorization attribute (like the existing cache endpoints). Ids under a sheet change on every rebuild; the sheet keeps its ReferenceID.
8. A deliberate EF leftover: SpecSheetTableNotes.CreatedByUserId relates to Users in EF but the DB has no FK.
