import { CreateSpecSheetTableCellRequest } from '../cells/create-spec-sheet-table-cell-request';

export type CreateSpecSheetTableRowRequest = {
  specSheetTableGroupId: number;
  tableRowTemplateId: number;
  rowNumber: number;
  referenceId: string;
  cellRequests: CreateSpecSheetTableCellRequest[];
};
