/**
 * How a table's columns are shared out: the rows' own cells first, then each column block in turn.
 * Block cells count their columns from their block's first column.
 */
export interface ColumnPlan {
  /** Every column of the table. */
  total: number;
  /** The table column each block starts on, by block id. */
  blockStarts: ReadonlyMap<number, number>;
  /** How many columns each block takes, by block id. */
  blockWidths: ReadonlyMap<number, number>;
}
