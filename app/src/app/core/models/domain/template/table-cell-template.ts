import { CellConfiguration } from '../configuration/cell-configuration';
import { TableCellType } from './table-cell-type';

export type TableCellTemplate = {
  id: number;
  tableRowTemplateId: number;
  tableCellTypeId: number;
  cellConfigurationId: number;
  columnNumber: number;
  rowSpan: number;
  colSpan: number;
  groupKey?: string;
  cellConfiguration: CellConfiguration;
  tableCellType: TableCellType;
};
