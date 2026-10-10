/**
 * Where a linked dropdown cell takes its choices from, in another table on the same sheet: either one
 * column of it (named by the template cell its cells were built from), giving the values that column
 * holds in every row, or one row of it, giving every value in that row. One of the two is set.
 */
export interface LinkedDropdownInstanceSettings {
  kind: 'LinkedDropdown';
  sourceSheetTableId: number | null;
  sourceTemplateCellId: number | null;
  /** For a row source. Null or absent for a column source. */
  sourceSheetRowId?: number | null;
}
