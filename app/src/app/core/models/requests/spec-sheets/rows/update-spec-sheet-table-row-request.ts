import { ManageSpecSheetTableCellsRequest } from '../cells/manage-spec-sheet-table-cells-request';

export type UpdateSpecSheetTableRowRequest = {
  id: number;
  rowNumber?: number;
  manageSpecSheetTableCellsRequest?: ManageSpecSheetTableCellsRequest;
};
