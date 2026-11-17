import { User } from '../access/user';
import { CellConfiguration } from '../configuration/cell-configuration';
import { CellProperties } from '../properties/cell/cell-properties';
import { TableCellType } from '../template/table-cell-type';
import { CellValue } from '../values/cell-value';

export type SpecSheetTableCell = {
  id: number;
  specSheetTableRowId: number;
  tableCellTypeId: number;
  cellConfigurationId: number;
  rowNumber: number;
  columnNumber: number;
  rowSpan: number;
  colSpan: number;
  referenceId: string;
  isDeleted: boolean;
  cellValue: CellValue;
  createdByUserId: number;
  createdDateTime: string;
  createdByUser?: User;
  lastModifiedByUserId?: number;
  lastModifiedDateTime?: string;
  lastModifiedByUser?: User;
  configurationOverride?: CellProperties;
  cellConfiguration: CellConfiguration;
  tableCellType: TableCellType;
};
