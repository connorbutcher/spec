import { CreateSpecSheetTableRequest } from './create-spec-sheet-table-request';
import { UpdateSpecSheetTableRequest } from './update-spec-sheet-table-request';

export type ManageSpecSheetTablesRequest = {
  tablesToCreate: CreateSpecSheetTableRequest[];
  tablesToUpdate: UpdateSpecSheetTableRequest[];
  tableIdsToDelete: number[];
};
