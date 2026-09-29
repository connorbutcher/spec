import { CellConfiguration } from './cell-configuration';
import { CellStyle } from './cell-style.model';

/**
 * A cell in a row. `column` is 1-based; the spans map onto CSS grid spans. The overrides hold only
 * what the cell changes from its cell type's defaults.
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
}
