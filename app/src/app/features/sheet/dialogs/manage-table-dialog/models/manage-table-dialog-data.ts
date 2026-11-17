import { PUSpecSheetTableCellBase } from '../../../models/cells/pu-spec-sheet-table-cell-base';
import { PUSpecSheetTableBase } from '../../../models/tables/pu-spec-sheet-table-base';

export type ManageTableDialogData = {
  table: PUSpecSheetTableBase;
  cellToConfigure?: PUSpecSheetTableCellBase;
};
