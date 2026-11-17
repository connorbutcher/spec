import { TableProperties } from '../../../domain/properties/table/table-properties';
import { CreateSpecSheetTableGroupRequest } from '../groups/create-spec-sheet-table-group-request';
import { CreateSpecSheetTableNoteRequest } from '../notes/create-spec-sheet-table-note-request';
import { CreateSpecSheetTableWidgetRequest } from './widgets/create-spec-sheet-table-widget-request';

export type CreateSpecSheetTableRequest = {
  specSheetId: number;
  tableTemplateId: number;
  title: string;
  displayOrder: number;
  referenceId: string;
  groupRequests: CreateSpecSheetTableGroupRequest[];
  widgetRequests: CreateSpecSheetTableWidgetRequest[];
  noteRequests: CreateSpecSheetTableNoteRequest[];
  configurationOverride?: TableProperties;
};
