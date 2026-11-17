import { PropertyChanged } from '../../../common/property-changed';
import { TableProperties } from '../../../domain/properties/table/table-properties';
import { ManageSpecSheetTableGroupsRequest } from '../groups/manage-spec-sheet-table-groups-request';
import { ManageSpecSheetTableNotesRequest } from '../notes/manage-spec-sheet-table-notes-request';
import { UpdateSpecSheetTableWidgetRequest } from './widgets/update-spec-sheet-table-widget-request';

export type UpdateSpecSheetTableRequest = {
  id: number;
  title?: string;
  displayOrder?: number;
  manageSpecSheetTableGroupsRequest?: ManageSpecSheetTableGroupsRequest;
  updateSpecSheetTableWidgetRequests?: UpdateSpecSheetTableWidgetRequest[];
  manageSpecSheetTableNotesRequest?: ManageSpecSheetTableNotesRequest;
  configurationOverride?: PropertyChanged<TableProperties | undefined>;
};
