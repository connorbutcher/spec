/**
 * A cell's new value. Set the field that matches the cell's kind; leaving it unset clears the cell.
 * A date is `yyyy-MM-dd`.
 */
export type CellValueRequest = {
  sheetCellId: number;
  text?: string | null;
  number?: number | null;
  date?: string | null;
  boolean?: boolean | null;
  optionId?: number | null;
  /** A labelled text cell's values by slot key, in the cell's order. A slot left out is cleared. */
  slots?: Record<string, string> | null;
}
