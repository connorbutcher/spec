/**
 * A cell's new value. Set the field that matches the cell's kind; leaving it unset clears the cell.
 * A date is `yyyy-MM-dd`.
 */
export interface CellValueRequest {
  sheetCellId: number;
  text?: string | null;
  number?: number | null;
  date?: string | null;
  boolean?: boolean | null;
  optionId?: number | null;
}
