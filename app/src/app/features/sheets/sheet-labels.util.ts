import { SheetSection } from './models/sheet-section.model';

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
