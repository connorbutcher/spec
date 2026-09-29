/** Settings for a number cell. Null means not set, or on an override, "use the default". */
export interface NumberCellConfiguration {
  kind: 'Number';
  decimalPlaces?: number | null;
  minValue?: number | null;
  maxValue?: number | null;
  unit?: string | null;
}
