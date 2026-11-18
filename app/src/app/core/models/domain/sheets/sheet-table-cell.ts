import { CellConfiguration } from '../configuration/cell-configuration';
import { CellProperties } from '../properties/cell/cell-properties';
import { TableCellType } from '../template/table-cell-type';
import { SheetCellValue } from './sheet-cell-value';

export type SheetTableCell = {
  id: number;
  sheetTableRowId: number;
  tableCellTemplateId: number;
  tableCellTypeId: number;
  cellConfigurationId: number;
  columnNumber: number;
  colSpan: number;
  rowSpan: number;
  referenceId: string;
  createdDateTime: string;
  value: SheetCellValue;
  configurationOverride?: CellProperties;
  cellConfiguration?: CellConfiguration;
  tableCellType?: TableCellType;
};
