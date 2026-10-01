/**
 * What is selected on the sheet: a table, optionally a section in it, and optionally a row in that
 * section. Selecting a row also selects its section, so both sets of actions are on offer.
 */
export interface SheetSelection {
  tableId: number;
  sectionId: number | null;
  rowId: number | null;
}
