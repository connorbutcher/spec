import { CellLayout } from '../../templates/models/cell-layout.model';

/** The cells of one row of a section, in the order they are laid out. */
export interface RowLayout {
  rowId: number;
  cells: CellLayout[];
}
