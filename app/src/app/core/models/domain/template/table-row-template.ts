import { Validator } from '../configuration/validator';
import { TableCellTemplate } from './table-cell-template';

export type TableRowTemplate = {
  id: number;
  tableGroupTemplateId: number;
  rowNumber: number;
  showContextMenu: boolean;
  canBeReordered: boolean;
  cellTemplates: TableCellTemplate[];
  validators: Validator[];
};
