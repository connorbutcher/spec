/**
 * How a table's columns are shared out: the rows' own cells first, then each column block in turn.
 * Block cells count their columns from their block's first column.
 */
export type ColumnPlan = {
  total: number;
  blockStarts: ReadonlyMap<number, number>;
  blockWidths: ReadonlyMap<number, number>;
}
