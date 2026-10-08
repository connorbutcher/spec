import { SheetChange } from './models/sheet-change.model';
import { SheetRow } from './models/sheet-row.model';
import { SheetSection } from './models/sheet-section.model';
import { SheetTable } from './models/sheet-table.model';

/**
 * A name for one section copy: its template name, plus the first text typed into it if there is any,
 * so copies of the same kind can be told apart, e.g. "Group · Intake valve".
 */
export function sectionLabel(section: SheetSection): string {
  const firstText = section.rows
    .flatMap((row) => row.cells)
    .map((cell) => cell.textValue)
    .find((text) => !!text);
  return firstText ? `${section.name} · ${firstText}` : section.name;
}

/** What to call a row when talking about it: the first text typed into it, or null when it has none. */
export function rowLabel(row: SheetRow): string | null {
  return row.cells.map((cell) => cell.textValue).find((text) => !!text) ?? null;
}

/** "v3 · 12 Sep 2026 · A. Smith": which publish changed something, when and by whom. */
export function changeLabel(change: SheetChange): string {
  const when = new Date(change.atUtc).toLocaleDateString(undefined, { dateStyle: 'medium' });
  return `${versionLabel(change.versionNumber)} · ${when} · ${change.userName}`;
}

/** "v3": how a published version is written wherever it is shown. */
export function versionLabel(versionNumber: number): string {
  return `v${versionNumber}`;
}

/** A UTC moment as the viewer's local date and time, e.g. "12 Sep 2026, 14:05". */
export function momentLabel(utc: string): string {
  return new Date(utc).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' });
}

/** What a table is called: the title typed for it, or its template's name until it has one. */
export function tableLabel(table: SheetTable): string {
  return table.title || table.templateName;
}
