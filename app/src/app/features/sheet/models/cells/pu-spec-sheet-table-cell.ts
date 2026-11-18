import { PUSpecSheetTableRow } from '../pu-spec-sheet-table-row';
import { PUSpecSheetTableCellBase } from './pu-spec-sheet-table-cell-base';
import { SheetTableCell } from '../../../../core/models/domain/sheets/sheet';

export class PUSpecSheetTableCell extends PUSpecSheetTableCellBase {
  constructor(cell: SheetTableCell, parent: PUSpecSheetTableRow) {
    super(cell, parent);
  }

  public override get originalValue(): any {
    return (this.cellValue as any).value;
  }
}
