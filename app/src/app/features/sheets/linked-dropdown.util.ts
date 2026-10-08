import { CellInstanceSettings } from './models/cell-instance-settings';
import { LinkedChoices } from './models/linked-choices.model';
import { SheetIndex } from './models/sheet-index.model';
import { Sheet } from './models/sheet.model';
import { tableLabel } from './sheet-labels.util';

const NO_OPTIONS: readonly string[] = [];
const UNSET: LinkedChoices = { status: 'unset', source: '', options: NO_OPTIONS };
const MISSING: LinkedChoices = { status: 'missing', source: '', options: NO_OPTIONS };

/** What a linked dropdown cell with these settings can offer on this sheet. */
export function linkedChoices(
  sheet: Sheet | null,
  index: SheetIndex,
  settings: CellInstanceSettings | null,
): LinkedChoices {
  const tableId = settings?.sourceSheetTableId ?? null;
  const templateCellId = settings?.sourceTemplateCellId ?? null;
  if (tableId === null || templateCellId === null) {
    return UNSET;
  }
  const table = index.tables.get(tableId);
  const column = table?.linkableColumns.find((candidate) => candidate.templateCellId === templateCellId);
  if (table === undefined || column === undefined) {
    return MISSING;
  }
  const source = sheet?.linkedSources.find(
    (candidate) => candidate.sheetTableId === tableId && candidate.templateCellId === templateCellId,
  );
  return {
    status: 'ready',
    source: `${tableLabel(table)} › ${column.label}`,
    options: source?.options ?? NO_OPTIONS,
  };
}

/** Whether two answers from `linkedChoices` say the same thing, so a cell that reads one needn't redraw. */
export function sameLinkedChoices(a: LinkedChoices | null, b: LinkedChoices | null): boolean {
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
export function linkedWarning(choices: LinkedChoices, value: string | null): string | null {
  if (choices.status === 'missing') {
    return 'The table or column this cell takes its choices from is no longer on the sheet. Choose another.';
  }
  if (choices.status === 'ready' && value && !choices.options.includes(value)) {
    return `"${value}" is no longer in ${choices.source}. It stays as it is until someone picks again.`;
  }
  return null;
}

/** What an empty linked dropdown cell says in place of a value, when it isn't simply waiting for a choice. */
export function linkedHint(choices: LinkedChoices): string | null {
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
export function linkedOptions(choices: LinkedChoices, value: string | null): readonly string[] {
  return value && !choices.options.includes(value) ? [value, ...choices.options] : choices.options;
}
