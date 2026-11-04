/**
 * The choices one source gives the linked dropdowns pointed at it, without repeats. A source is a column
 * of a table (`templateCellId` is set: the values it holds, top to bottom) or a row of it (`sheetRowId` is
 * set: the values in the row after the one that names it). Only sources some cell is linked to are sent.
 */
export type SheetLinkedSource = {
  sheetTableId: number;
  templateCellId: number | null;
  sheetRowId: number | null;
  options: string[];
}
