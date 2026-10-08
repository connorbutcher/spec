import { SheetIndex } from './models/sheet-index.model';
import { SheetLock } from './models/sheet-lock.model';
import { SheetSection } from './models/sheet-section.model';
import { Sheet } from './models/sheet.model';

const HUB_ERROR_PREFIX = 'HubException: ';

/**
 * The sheet with each row someone is in (but has not changed) shown as locked to them. A row that is
 * already locked because someone changed it keeps that lock.
 *
 * Only a row that gains a lock, and the sections and table around it, are new objects; everything else,
 * and the whole sheet when no row gains one, is the object it was. That matters: the screen re-renders
 * what is a new object and nothing else.
 */
export function withRowCheckouts(sheet: Sheet, locks: ReadonlyMap<number, SheetLock>): Sheet {
  if (locks.size === 0) {
    return sheet;
  }
  const tables = mapChanged(sheet.tables, (table) => {
    const sections = mapChanged(table.sections, (section) =>
      sectionWithRowCheckouts(section, locks),
    );
    return sections === table.sections ? table : { ...table, sections };
  });
  return tables === sheet.tables ? sheet : { ...sheet, tables };
}

function sectionWithRowCheckouts(
  section: SheetSection,
  locks: ReadonlyMap<number, SheetLock>,
): SheetSection {
  const rows = mapChanged(section.rows, (row) => {
    const lock = row.lock === null ? locks.get(row.id) : undefined;
    return lock === undefined ? row : { ...row, lock };
  });
  const sections = mapChanged(section.sections, (child) => sectionWithRowCheckouts(child, locks));
  return rows === section.rows && sections === section.sections
    ? section
    : { ...section, rows, sections };
}

/** `items.map(change)`, but the array itself when `change` gave every item back as it was. */
function mapChanged<T>(items: T[], change: (item: T) => T): T[] {
  const changed = items.map(change);
  return changed.every((item, index) => item === items[index]) ? items : changed;
}

/** The row a cell is in, or null when the cell isn't on the sheet. */
export function rowIdOfCell(index: SheetIndex, cellId: number): number | null {
  for (const row of index.rows.values()) {
    if (row.cells.some((cell) => cell.id === cellId)) {
      return row.id;
    }
  }
  return null;
}

/**
 * The reason the server gave for refusing a call over the live connection. SignalR wraps it in its own
 * sentence ("An unexpected error occurred invoking … HubException: <reason>"); this takes the reason out.
 */
export function hubRefusalMessage(failure: unknown): string {
  const message = failure instanceof Error ? failure.message : '';
  const at = message.indexOf(HUB_ERROR_PREFIX);
  return at >= 0
    ? message.slice(at + HUB_ERROR_PREFIX.length)
    : "This row couldn't be checked out to you.";
}
