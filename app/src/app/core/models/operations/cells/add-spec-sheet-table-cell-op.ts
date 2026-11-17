import { CellProperties } from '../../domain/properties/cell/cell-properties';

export type AddSpecSheetTableCellOp = {
  $type: 'AddSpecSheetTableCellOp';
  tempId: number;
  specSheetTableRowId: number;
  tableCellTypeId: number;
  cellConfigurationId?: number;
  rowNumber: number;
  columnNumber: number;
  rowSpan: number;
  colSpan: number;
  referenceId: string;
  configurationOverride?: CellProperties;
};
