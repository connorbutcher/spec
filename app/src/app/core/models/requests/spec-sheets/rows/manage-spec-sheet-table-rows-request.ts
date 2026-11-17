import { CreateSpecSheetTableRowRequest } from './create-spec-sheet-table-row-request';
import { UpdateSpecSheetTableRowRequest } from './update-spec-sheet-table-row-request';

export type ManageSpecSheetTableRowsRequest = {
  rowsToCreate: CreateSpecSheetTableRowRequest[];
  rowsToUpdate: UpdateSpecSheetTableRowRequest[];
  rowIdsToDelete: number[];
};
