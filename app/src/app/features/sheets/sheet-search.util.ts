import { CellSettingsUtil } from '../templates/cell-settings.util';
import { CellKinds } from '../templates/models/cell-kinds';
import { CellLayout } from '../templates/models/cell-layout.model';
import { CellType } from '../templates/models/cell-type.model';
import { SectionLayout } from '../templates/models/section-layout.model';
import { CellValueUtil } from './cell-value.util';
import { SheetCell } from './models/sheet-cell.model';
import { SheetSearchTarget } from './models/sheet-search-target.model';
import { SheetIndex } from './models/sheet-index.model';
import { Sheet } from './models/sheet.model';
import { SheetAnchorUtil } from './sheet-anchor.util';
import { SheetLabelsUtil } from './sheet-labels.util';
import { SheetLayoutUtil } from './sheet-layout.util';

export class SheetSearchUtil {
  /**
   * Everything on the sheet a search can find, in the order it is on screen: each table's heading (its
   * title and template), then its cells as the grid lays them out. A heading or group cell is found by its
   * caption, a value cell by its value as it is shown (a number to its decimal places with its unit, a
   * dropdown's option text, a date). An empty cell, a checkbox and the status column have nothing to find.
   */
  public static searchTargets(sheet: Sheet | null, index: SheetIndex, cellTypes: ReadonlyMap<number, CellType>): SheetSearchTarget[] {
    const targets: SheetSearchTarget[] = [];
    for (const table of sheet?.tables ?? []) {
      targets.push({
        key: SheetAnchorUtil.anchorKey('table', table.publicId),
        text: `${SheetLabelsUtil.tableLabel(table)}\n${table.templateName}`.toLowerCase(),
      });
      for (const section of SheetLayoutUtil.layoutSheetTable(table).sections) {
        SheetSearchUtil.addCells(section, index, cellTypes, targets);
      }
    }
    return targets;
  }

  /** The keys of the targets whose text holds `query`, ignoring case. Nothing for a blank query. */
  public static matchingKeys(targets: SheetSearchTarget[], query: string): string[] {
    const needle = query.trim().toLowerCase();
    if (needle === '') {
      return [];
    }
    return targets.filter((target) => target.text.includes(needle)).map((target) => target.key);
  }

  /** "3 of 12", or "No matches". */
  public static matchCountLabel(activeIndex: number, count: number): string {
    return count === 0 ? 'No matches' : `${activeIndex + 1} of ${count}`;
  }

  /** A section's own cells, row by row, then its sub-sections: the order `SheetGridSection` draws them. */
  private static addCells(section: SectionLayout, index: SheetIndex, cellTypes: ReadonlyMap<number, CellType>, targets: SheetSearchTarget[]): void {
    for (const layout of section.cells) {
      const cell = index.cells.get(layout.cell.id);
      const text = cell === undefined ? '' : SheetSearchUtil.cellText(layout, cell, cellTypes);
      if (cell !== undefined && text !== '') {
        targets.push({ key: SheetAnchorUtil.anchorKey('cell', cell.publicId), text: text.toLowerCase() });
      }
    }
    for (const child of section.children) {
      SheetSearchUtil.addCells(child, index, cellTypes, targets);
    }
  }

  /** What a grid cell shows: a heading's caption, or a value cell's value as text. */
  private static cellText(layout: CellLayout, cell: SheetCell, cellTypes: ReadonlyMap<number, CellType>): string {
    const cellType = cellTypes.get(layout.cell.cellTypeId);
    if (cellType === undefined) {
      return '';
    }
    if (CellKinds.isDisplayOnly(cellType.kind)) {
      return layout.cell.caption ?? '';
    }
    return CellValueUtil.displayValue(
      cell,
      cellType,
      CellSettingsUtil.effectiveConfiguration(cellType.configuration, layout.cell.configurationOverride),
    );
  }
}
