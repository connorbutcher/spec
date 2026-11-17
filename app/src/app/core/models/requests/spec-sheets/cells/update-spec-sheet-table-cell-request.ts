import { PropertyChanged } from '../../../common/property-changed';
import { CellProperties } from '../../../domain/properties/cell/cell-properties';
import { CellValue } from '../../../domain/values/cell-value';

export type UpdateSpecSheetTableCellRequest = {
  id: number;
  rowNumber?: number;
  columnNumber?: number;
  cellValue?: PropertyChanged<CellValue | undefined>;
  configurationOverride?: PropertyChanged<CellProperties | undefined>;
};
