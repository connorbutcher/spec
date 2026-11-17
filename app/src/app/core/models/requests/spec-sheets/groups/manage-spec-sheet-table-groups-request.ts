import { CreateSpecSheetTableGroupRequest } from './create-spec-sheet-table-group-request';
import { UpdateSpecSheetTableGroupRequest } from './update-spec-sheet-table-group-request';

export type ManageSpecSheetTableGroupsRequest = {
  groupsToCreate: CreateSpecSheetTableGroupRequest[];
  groupsToUpdate: UpdateSpecSheetTableGroupRequest[];
  groupIdsToDelete: number[];
};
