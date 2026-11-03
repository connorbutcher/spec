import { SheetActionTarget } from './models/sheet-action-target.model';
import { SheetIndex } from './models/sheet-index.model';
import { SheetRow } from './models/sheet-row.model';
import { SheetSection } from './models/sheet-section.model';
import { SheetSelection } from './models/sheet-selection.model';
import { SheetTable } from './models/sheet-table.model';
import { sectionAncestors, sectionSiblings } from './sheet-index.util';
import { sectionLabel, tableLabel } from './sheet-labels.util';
import { otherUsersLock } from './sheet-lock.util';

/**
 * What move and remove act on for a selection: the deepest thing selected that can be moved by itself.
 * A row counts only when its section has several, as a section with one row is moved and removed as a
 * whole. The header section stays where it is, so it gives no target. `tables` are the sheet's, in order.
 */
export function sheetActionTarget(
  index: SheetIndex,
  tables: SheetTable[],
  selection: SheetSelection | null,
): SheetActionTarget | null {
  const table = selection === null ? undefined : index.tables.get(selection.tableId);
  if (selection === null || table === undefined) {
    return null;
  }
  const section = index.sections.get(selection.sectionId ?? -1);
  if (section === undefined) {
    return tableTarget(table, tables);
  }
  const row = index.rows.get(selection.rowId ?? -1);
  if (row !== undefined && row.canRemove && section.rows.length > 1) {
    return rowTarget(row, section);
  }
  return section.role === 'Header' ? null : sectionTarget(index, section);
}

/**
 * The nearest section, from the selected one outwards, that can take new sections: where the context
 * bar's add puts them. Null when nothing around the selection can.
 */
export function addableSection(
  index: SheetIndex,
  selection: SheetSelection | null,
): SheetSection | null {
  const selected = index.sections.get(selection?.sectionId ?? -1);
  if (selected === undefined) {
    return null;
  }
  const outwards = [selected, ...sectionAncestors(index, selected.id).reverse()];
  return outwards.find((section) => section.addableSections.length > 0) ?? null;
}

function rowTarget(row: SheetRow, section: SheetSection): SheetActionTarget {
  const isFree = otherUsersLock(row.lock) === null;
  const position = section.rows.indexOf(row);
  return {
    kind: 'row',
    id: row.id,
    canMoveUp: isFree && position > 0,
    canMoveDown: isFree && position < section.rows.length - 1,
    canRemove: isFree,
    removeQuestion: 'Remove this row?',
  };
}

function sectionTarget(index: SheetIndex, section: SheetSection): SheetActionTarget {
  const siblings = sectionSiblings(index, section.id);
  const first = siblings[0]?.role === 'Header' ? 1 : 0;
  const position = siblings.findIndex((candidate) => candidate.id === section.id);
  return {
    kind: 'section',
    id: section.id,
    canMoveUp: position > first,
    canMoveDown: position >= 0 && position < siblings.length - 1,
    canRemove: section.canRemove,
    removeQuestion: `Remove ${sectionLabel(section)}? Its rows and sections go too.`,
  };
}

function tableTarget(table: SheetTable, tables: SheetTable[]): SheetActionTarget {
  const isFree = otherUsersLock(table.lock) === null;
  const position = tables.findIndex((candidate) => candidate.id === table.id);
  return {
    kind: 'table',
    id: table.id,
    canMoveUp: isFree && position > 0,
    canMoveDown: isFree && position >= 0 && position < tables.length - 1,
    canRemove: isFree,
    removeQuestion: `Remove ${tableLabel(table)}? Everything in it goes too.`,
  };
}
