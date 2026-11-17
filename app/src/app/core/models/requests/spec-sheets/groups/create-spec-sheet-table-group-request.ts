import { CreateSpecSheetTableRowRequest } from '../rows/create-spec-sheet-table-row-request';
import { CreateSpecSheetTableGroupWidgetRequest } from './widgets/create-spec-sheet-table-group-widget-request';

export type CreateSpecSheetTableGroupRequest = {
  specSheetTableId: number;
  tableGroupTemplateId: number;
  linkedSpecSheetTableGroupId?: number;
  order: number;
  referenceId: string;
  rowRequests: CreateSpecSheetTableRowRequest[];
  childGroupRequests: CreateSpecSheetTableGroupRequest[];
  widgetRequests: CreateSpecSheetTableGroupWidgetRequest[];
};
