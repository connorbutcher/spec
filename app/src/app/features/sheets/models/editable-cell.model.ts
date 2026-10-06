import { CellConfiguration } from '../../templates/models/cell-configuration';
import { CellType } from '../../templates/models/cell-type.model';
import { SheetCell } from './sheet-cell.model';

/** Everything an editor needs for a cell the viewer can change: the cell, its type and its effective settings. */
export interface EditableCell {
  cell: SheetCell;
  cellType: CellType;
  configuration: CellConfiguration;
}
