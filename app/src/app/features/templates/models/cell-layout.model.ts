import { GridStyle } from './grid-style';
import { TemplateCell } from './template-cell.model';

/** Where a cell sits in its section's subgrid. */
export interface CellLayout {
  cell: TemplateCell;
  rowId: number;
  style: GridStyle;
}
