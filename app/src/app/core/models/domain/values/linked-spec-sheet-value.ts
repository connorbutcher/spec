import { CellValue } from './cell-value';

export type LinkedSpecSheetValue = CellValue & {
  tableReferenceId?: string;
  linkedSpecSheetTypeId?: number;
  linkedSpecSheetPhaseId?: number;
  linkedReferenceId?: string;
};
