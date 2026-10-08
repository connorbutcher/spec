/**
 * Where a linked dropdown cell takes its choices from: one column of another table on the same sheet.
 * The column is named by the template cell its cells were built from.
 */
export interface LinkedDropdownInstanceSettings {
  kind: 'LinkedDropdown';
  sourceSheetTableId: number | null;
  sourceTemplateCellId: number | null;
}
