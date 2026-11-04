/** Where a cell sits in its row: its 1-based column and how many columns and rows it spans. */
export type CellPlacement = {
  column: number;
  columnSpan: number;
  rowSpan: number;
}
