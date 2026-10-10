/**
 * Where a linked dropdown cell takes its choices from: one column of another table on the same sheet.
 * The column is named by the template cell its cells were built from.
 */
export interface LinkedDropdownInstanceSettings {
  kind: 'LinkedDropdown';
  sourceSheetTableId: number | null;
  sourceTemplateCellId: number | null;
  /**
   * True when the cell also shows the other values of the row its choice was picked from. They are read
   * from that row each time, never stored with this cell. Null or absent when off.
   */
  includeRowValues?: boolean | null;
}
