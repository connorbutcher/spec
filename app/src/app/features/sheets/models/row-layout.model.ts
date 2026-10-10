import { CellLayout } from '../../templates/models/cell-layout.model';
import { GridStyle } from '../../templates/models/grid-style';

/** The cells of one row of a section, in the order they are laid out, and where the row's status sits. */
export interface RowLayout {
  rowId: number;
  cells: CellLayout[];
  /** The row's place in the table's status column. */
  statusStyle: GridStyle;
}
