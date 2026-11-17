import { PropertyChanged } from '../../../common/property-changed';
import { ManageSpecSheetTableRowsRequest } from '../rows/manage-spec-sheet-table-rows-request';
import { ManageSpecSheetTableGroupsRequest } from './manage-spec-sheet-table-groups-request';
import { UpdateSpecSheetTableGroupWidgetRequest } from './widgets/update-spec-sheet-table-group-widget-request';

export type UpdateSpecSheetTableGroupRequest = {
  id: number;
  order?: number;
  linkedSpecSheetTableGroupId?: PropertyChanged<number | undefined>;
  manageSpecSheetTableRowsRequest?: ManageSpecSheetTableRowsRequest;
  manageSpecSheetTableGroupsRequest?: ManageSpecSheetTableGroupsRequest;
  updateSpecSheetTableGroupWidgetRequests: UpdateSpecSheetTableGroupWidgetRequest[];
};
