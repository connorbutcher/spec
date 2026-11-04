import { CellInstanceSettings } from './models/cell-instance-settings';
import { LinkedChoices } from './models/linked-choices.model';
import { SheetIndex } from './models/sheet-index.model';
import { Sheet } from './models/sheet.model';
import { SheetLabelsUtil } from './sheet-labels.util';

const NO_OPTIONS: readonly string[] = [];
const UNSET: LinkedChoices = { status: 'unset', source: '', options: NO_OPTIONS };
const MISSING: LinkedChoices = { status: 'missing', source: '', options: NO_OPTIONS };

export class LinkedDropdownUtil {
  /** What a linked dropdown cell with these settings can offer on this sheet. */
  public static linkedChoices(sheet: Sheet | null, index: SheetIndex, settings: CellInstanceSettings | null): LinkedChoices {
    const tableId = settings?.sourceSheetTableId ?? null;
    const templateCellId = settings?.sourceTemplateCellId ?? null;
    const rowId = settings?.sourceSheetRowId ?? null;
    if (tableId === null || (templateCellId === null) === (rowId === null)) {
      return UNSET;
    }
    const table = index.tables.get(tableId);
    const part =
      rowId === null
        ? table?.linkableColumns.find((candidate) => candidate.templateCellId === templateCellId)
        : table?.linkableRows.find((candidate) => candidate.sheetRowId === rowId);
    if (table === undefined || part === undefined) {
      return MISSING;
    }
    const source = sheet?.linkedSources.find(
      (candidate) =>
        candidate.sheetTableId === tableId &&
        candidate.templateCellId === templateCellId &&
        candidate.sheetRowId === rowId,
    );
    return {
      status: 'ready',
      source: `${SheetLabelsUtil.tableLabel(table)} › ${part.label}`,
      options: source?.options ?? NO_OPTIONS,
    };
  }

  /** Whether two answers from `linkedChoices` say the same thing, so a cell that reads one needn't redraw. */
  public static sameLinkedChoices(a: LinkedChoices | null, b: LinkedChoices | null): boolean {
    return (
      a === b ||
      (a !== null &&
        b !== null &&
        a.status === b.status &&
        a.source === b.source &&
        a.options === b.options)
    );
  }

  /**
   * What is wrong with a linked dropdown cell, in words, or null when nothing is. A value stays as it was
   * chosen when the column it came from changes, so it can end up no longer among the choices.
   */
  public static linkedWarning(choices: LinkedChoices, value: string | null): string | null {
    if (choices.status === 'missing') {
      return 'What this cell takes its choices from is no longer on the sheet. Choose another source.';
    }
    if (choices.status === 'ready' && value && !choices.options.includes(value)) {
      return `"${value}" is no longer in ${choices.source}. It stays as it is until someone picks again.`;
    }
    return null;
  }

  /** What an empty linked dropdown cell says in place of a value, when it isn't simply waiting for a choice. */
  public static linkedHint(choices: LinkedChoices): string | null {
    switch (choices.status) {
      case 'unset': {
        return 'No source chosen';
      }
      case 'missing': {
        return 'Source removed';
      }
      default: {
        return null;
      }
    }
  }

  /** The choices to list for a cell: the column's values, with the cell's own value first if it is no longer one of them. */
  public static linkedOptions(choices: LinkedChoices, value: string | null): readonly string[] {
    return value && !choices.options.includes(value) ? [value, ...choices.options] : choices.options;
  }
}
