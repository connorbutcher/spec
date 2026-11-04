import { CellConfiguration } from './cell-configuration';
import { CellStyle } from './cell-style.model';

/** The body for replacing what a cell changes from its cell type's defaults. Null means nothing. */
export type UpdateTemplateCellOverridesRequest = {
  configurationOverride: CellConfiguration | null;
  styleOverride: CellStyle | null;
}
