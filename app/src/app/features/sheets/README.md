# Sheet feature

The screen at `/phases/:phaseId/sheets/:sheetTypeId`: one spec sheet for a phase, built from table
templates, edited cell by cell, and published as numbered versions.

## Folder map

Every folder sits directly in `sheets/`. An indented entry is used by the entry above it.

```
sheets/
  sheet.store.ts            State for the open sheet. Provided by SheetPage, one per open sheet.
  sheets-api.ts             Write calls. Every one returns the whole live sheet.
  models/                   One interface or type per file. Sheet → SheetTable → SheetSection → SheetRow → SheetCell.

  sheet-page/               The route component. Header, toolbar, the list of tables (drag to reorder).
  sheet-switcher/           Tabs for the phase's other sheets.
  sheet-toolbar/            Version picker, compare picker, refresh, add table, publish.
    sheet-version-picker/     Live, a published version, or a date and time (past views are read-only).
    sheet-compare-picker/     "Changes since vN": turns the changed-in marks on.
    sheet-publish/            Publish button with its note popover.
  sheet-load-error/         "Couldn't load the sheet" with a retry.

  sheet-table-card/         One table: title, add menu, remove, the action bar and the grid.
  sheet-action-bar/         Move and remove for the selected section and row.
  sheet-add-menu/           The one add button for a table or a group section.
  sheet-grid/               The table as one CSS grid. Computes the layout.
  sheet-grid-section/       A section: an invisible subgrid holding its cells and child sections.
  sheet-grid-column-block/  Controls for one copy of a column block on a horizontal table.
  sheet-grid-cell/          One cell: decides whether it is a caption, an editor or a plain value.
    sheet-cell-editor/        Picks the editor for the cell's kind.
      sheet-text-cell/ sheet-number-cell/ sheet-date-cell/ sheet-checkbox-cell/ sheet-dropdown-cell/
    sheet-cell-value/         The read-only value.
    sheet-cell-marks/         Row-changed bar, cell-changed corner, lock icon.
  sheet-change-tag/         The "vN" tag on a changed section or column block.

  sheet-index.util.ts       Lookups by id (tables, sections, rows, cells) and tree questions.
  sheet-layout.util.ts      Turns a SheetTable into grid placements, reusing the template layout.
  sheet-sticky.util.ts      Pinned columns for horizontal tables.
  cell-value.util.ts        Cell value ⇄ request, and value → display text.
  sheet-labels.util.ts      Every label written in more than one place.
  sheet-lock.util.ts        "Is someone else editing this?"
  sheet-confirm.util.ts     The remove confirmation popup.
  sheet-url.util.ts         The address a sheet is read from.
  cell-field.scss           Mixin that makes a PrimeNG control fill its cell with no border.
  sheet-structure.fixture.ts  Test data builders.
```

Layout, cell types and cell settings come from `features/templates` (`template-layout.util.ts`,
`cell-settings.util.ts`, `models/`), so a sheet table is laid out exactly like its template.

## State flow

```
route params ─► SheetStore.target ─► httpResource(sheet) ─► sheet() ─► index()  (maps by id)
                     view (live | version | date) ─┘                └► components read by id

user action ─► component ─► store method ─► SheetsApi ─► whole live sheet comes back
                                              └► reuseUnchanged(previous, next) ─► sheetResource.set
```

- **Reads** are `httpResource`s in the store. **Writes** go through `SheetsApi` and are queued by
  `SheetStore.run`, one at a time, in the order they were made. A failure sets `store.error()` and
  reloads the sheet.
- **Every write replaces the sheet**, but `reuseUnchanged` (`shared/reuse-unchanged.util.ts`) keeps
  the previous objects for everything that is deep-equal. `SheetGrid.layout` does the same for the
  layout. Signals, inputs and `@for` items compare by identity, so only what changed re-renders. Keep
  this in mind: never mutate a sheet object, and don't put a fresh object or array in a hot `computed`
  unless it really changed.
- **Components find their data by id.** A grid component gets a layout item as its input and looks
  the matching sheet item up in `store.index()`. It never walks the tree.
- **Selection** (`store.selection()`) is a table, optionally a section, optionally a row. It clears
  itself when what it points at is no longer on the sheet.
- **Editing rules** live in `SheetGridCell.editable`: the sheet is live, the cell takes a value, and
  nobody else holds the row's lock. Editors keep what is being typed in a `linkedSignal` seeded from
  the cell, and emit `changed` only when the value really differs.
- **One editor at a time.** A cell that can be edited shows its plain value (`sheet-cell-value`) and
  is a tab stop. Pressing on it or tabbing to it makes it `store.editingCellId()`, which swaps in its
  editor and moves focus into it; the cell that had the editor goes back to its plain value. This is
  what keeps opening a sheet fast: a PrimeNG control per cell was about 85% of the time to open one.
  The plain value is styled to sit exactly where the control's text sits (`sheet-cell-value.scss`), so
  nothing moves on the swap. Checkboxes are the exception and always show their control.

## Add a cell kind

1. Agree the API side first (the `CellKind` name, its configuration, how the value is stored). This
   is a contract change.
2. In `features/templates/models`: add the name to `cell-kind.ts`, an entry in `cell-kinds.ts`, and a
   configuration interface joined into `cell-configuration.ts`.
3. In `cell-value.util.ts`: add a `case` to `valueRequest` (which request field carries the value) and
   to `displayValue` (how it reads as plain text). Add both to `cell-value.util.spec.ts`. If the value
   needs a new field, add it to `CellValueRequest` and `SheetCell`.
4. Create `sheet-<kind>-cell/` with `.ts`, `.html` and `.scss`. Copy the shape of `sheet-number-cell`:
   inputs `cell`, `label` (and `configuration` if it has settings), outputs `started` and `changed`,
   a `linkedSignal` for the draft value, a PrimeNG control, and `@include field.cell-field` on `:host`.
   The cell moves focus into the first `input`, `textarea` or `[role="combobox"]` it finds in the
   editor, so the control must have one.
5. Add its `@case` to `sheet-cell-editor.html` and its import to `sheet-cell-editor.ts`. That is the
   only place a kind is mapped to an editor.
6. Check its plain value lines up with its control: open a sheet, click into the cell and back out,
   and nothing should shift. If it doesn't read well as text (as a checkbox doesn't), add a branch to
   `sheet-cell-value`, or keep its control always on screen in `SheetGridCell.showsEditor`.

## Add an add, move or remove action

1. `SheetsApi`: a method that returns `Promise<Sheet>`.
2. `SheetStore`: a method that calls `this.run(() => this.api.…)`. To select what was added, compare
   the index before and after with `addedId`. Add a test to `sheet.store.spec.ts`.
3. The control, always a PrimeNG button disabled while `store.isBusy()`:
   - **Add**: a choice in `SheetAddMenu.choices`. What can be added, and whether it is at its maximum,
     comes from the API's `addable…` lists (`canAdd`), not from rules in the UI.
   - **Move or remove of the selection**: a button in `SheetActionBar`, enabled from a `computed`.
   - **Move or remove of one item with its own controls** (a table, a column block): in that
     component.
   - Confirm a removal with `confirmRemoval`; the page holds the single `p-confirmpopup`.

## Checks

`npm run lint` and `npm test -- --watch=false` from `app/`. The lint rules enforce the project conventions (one class
or interface per file, component folder layout, PrimeNG controls, signals, no subscriptions in
components).
