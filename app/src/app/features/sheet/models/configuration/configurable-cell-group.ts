import { TableCellType } from '../../../../core/models/domain/template/table-cell-type';
import { PUSpecSheetTableCellBase } from '../cells/pu-spec-sheet-table-cell-base';

export type ConfigurableCellGroup = {
  uid: string;
  rowNumber: number;
  columnNumber: number;
  tableCellType: TableCellType;
  cells: PUSpecSheetTableCellBase[];
};
