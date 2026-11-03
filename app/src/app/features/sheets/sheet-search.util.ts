import { effectiveConfiguration } from '../templates/cell-settings.util';
import { isDisplayOnly } from '../templates/models/cell-kinds';
import { CellLayout } from '../templates/models/cell-layout.model';
import { CellType } from '../templates/models/cell-type.model';
import { SectionLayout } from '../templates/models/section-layout.model';
import { displayValue } from './cell-value.util';
import { SheetSearchTarget } from './models/sheet-search-target.model';
import { SheetIndex } from './models/sheet-index.model';
import { Sheet } from './models/sheet.model';
import { tableLabel } from './sheet-labels.util';
import { layoutSheetTable } from './sheet-layout.util';

/** The `data-search-key` of a table's heading line. */
export function tableSearchKey(tableId: number): string {
  return `table:${tableId}`;
}

/** The `data-search-key` of a cell on the grid. */
export function cellSearchKey(cellId: number): string {
  return `cell:${cellId}`;
}

/**
 * Everything on the sheet a search can find, in the order it is on screen: each table's heading (its
 * title and template), then its cells as the grid lays them out. A heading or group cell is found by its
 * caption, a value cell by its value as it is shown (a number to its decimal places with its unit, a
 * dropdown's option text, a date). An empty cell, a checkbox and the status column have nothing to find.
 */
export function searchTargets(
  sheet: Sheet | null,
  index: SheetIndex,
  cellTypes: ReadonlyMap<number, CellType>,
): SheetSearchTarget[] {
  const targets: SheetSearchTarget[] = [];
  for (const table of sheet?.tables ?? []) {
    targets.push({
      key: tableSearchKey(table.id),
      text: `${tableLabel(table)}\n${table.templateName}`.toLowerCase(),
    });
    for (const section of layoutSheetTable(table).sections) {
      addCells(section, index, cellTypes, targets);
    }
  }
  return targets;
}

/** The keys of the targets whose text holds `query`, ignoring case. Nothing for a blank query. */
export function matchingKeys(targets: SheetSearchTarget[], query: string): string[] {
  const needle = query.trim().toLowerCase();
  if (needle === '') {
    return [];
  }
  return targets.filter((target) => target.text.includes(needle)).map((target) => target.key);
}

/** "3 of 12", or "No matches". */
export function matchCountLabel(activeIndex: number, count: number): string {
  return count === 0 ? 'No matches' : `${activeIndex + 1} of ${count}`;
}

/** A section's own cells, row by row, then its sub-sections: the order `SheetGridSection` draws them. */
function addCells(
  section: SectionLayout,
  index: SheetIndex,
  cellTypes: ReadonlyMap<number, CellType>,
  targets: SheetSearchTarget[],
): void {
  for (const layout of section.cells) {
    const text = cellText(layout, index, cellTypes);
    if (text !== '') {
      targets.push({ key: cellSearchKey(layout.cell.id), text: text.toLowerCase() });
    }
  }
  for (const child of section.children) {
    addCells(child, index, cellTypes, targets);
  }
}

/** What a grid cell shows: a heading's caption, or a value cell's value as text. */
function cellText(
  layout: CellLayout,
  index: SheetIndex,
  cellTypes: ReadonlyMap<number, CellType>,
): string {
  const cellType = cellTypes.get(layout.cell.cellTypeId);
  if (cellType === undefined) {
    return '';
  }
  if (isDisplayOnly(cellType.kind)) {
    return layout.cell.caption ?? '';
  }
  const cell = index.cells.get(layout.cell.id);
  return cell === undefined
    ? ''
    : displayValue(
        cell,
        cellType,
        effectiveConfiguration(cellType.configuration, layout.cell.configurationOverride),
      );
}
