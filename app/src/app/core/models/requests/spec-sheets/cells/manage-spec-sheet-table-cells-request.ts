import { CreateSpecSheetTableCellRequest } from './create-spec-sheet-table-cell-request';
import { UpdateSpecSheetTableCellRequest } from './update-spec-sheet-table-cell-request';

export type ManageSpecSheetTableCellsRequest = {
  cellsToCreate: CreateSpecSheetTableCellRequest[];
  cellsToUpdate: UpdateSpecSheetTableCellRequest[];
  cellIdsToDelete: number[];
};
