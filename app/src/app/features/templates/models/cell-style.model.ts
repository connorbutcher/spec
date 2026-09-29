import { CellTextAlign } from './cell-text-align';

/**
 * How a cell looks, whatever its kind. Null means the app's default, or on an override, "use the cell
 * type's value". Colours are hex values such as `#1f2937`.
 */
export interface CellStyle {
  bold?: boolean | null;
  italic?: boolean | null;
  align?: CellTextAlign | null;
  textColor?: string | null;
  backgroundColor?: string | null;
}
