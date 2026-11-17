import { SpecSheetTableCell } from '../../../../core/models/domain/spec-sheet/spec-sheet-table-cell';
import { PUSpecSheetTableRow } from '../pu-spec-sheet-table-row';
import { PUSpecSheetTableCellBase } from './pu-spec-sheet-table-cell-base';

export class PUSpecSheetTableCell extends PUSpecSheetTableCellBase {
  constructor(cell: SpecSheetTableCell, parent: PUSpecSheetTableRow) {
    super(cell, parent);
  }

  public override get originalValue(): any {
    return (this.cellValue as any).value;
  }
}
