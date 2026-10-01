import { CellConfiguration } from './cell-configuration';
import { CellStyle } from './cell-style.model';

/**
 * A cell in a row, among the row's own cells or in a column block. `column` is 1-based; the spans
 * map onto CSS grid spans. The overrides hold only what the cell changes from its cell type's
 * defaults.
 */
export interface TemplateCell {
  id: number;
  cellTypeId: number;
  column: number;
  rowSpan: number;
  columnSpan: number;
  caption: string | null;
  isRequired: boolean;
  configurationOverride: CellConfiguration | null;
  styleOverride: CellStyle | null;
  /** The column block the cell belongs to, or null. In a block, `column` counts from its first column. */
  columnBlockId: number | null;
}
