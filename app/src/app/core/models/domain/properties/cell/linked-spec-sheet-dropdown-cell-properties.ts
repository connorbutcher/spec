import { CellProperties } from './cell-properties';

export type LinkedSpecSheetDropdownCellProperties = CellProperties & {
  defaultValue?: string;
  linkedSpecSheetTypeId?: number;
  linkedSpecSheetPhaseId?: number;
  tableReferenceId?: string;
  rowReferenceId?: string;
  tableCellTypeKey?: string;
  lockToLatestPublished?: boolean;
};
