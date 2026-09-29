/** Settings for a number dropdown cell, whose options are all numbers. */
export interface NumberDropdownCellConfiguration {
  kind: 'NumberDropdown';
  decimalPlaces?: number | null;
  unit?: string | null;
}
