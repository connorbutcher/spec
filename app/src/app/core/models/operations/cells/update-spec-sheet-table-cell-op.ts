import { PropertyChanged } from '../../common/property-changed';
import { CellProperties } from '../../domain/properties/cell/cell-properties';

export type UpdateSpecSheetTableCellOp = {
  $type: 'UpdateSpecSheetTableCellOp';
  id: number;
  rowNumber?: number;
  columnNumber?: number;
  configurationOverride?: PropertyChanged<CellProperties | undefined>;
};
