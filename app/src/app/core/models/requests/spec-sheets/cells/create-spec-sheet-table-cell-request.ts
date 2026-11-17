import { CellProperties } from '../../../domain/properties/cell/cell-properties';
import { CellValue } from '../../../domain/values/cell-value';

export type CreateSpecSheetTableCellRequest = {
  specSheetTableGroupId: number;
  specSheetTableRowId: number;
  tableCellTypeId: number;
  cellConfigurationId: number;
  rowNumber: number;
  columnNumber: number;
  colSpan: number;
  rowSpan: number;
  referenceId: string;
  value: CellValue;
  configurationOverride?: CellProperties;
};
