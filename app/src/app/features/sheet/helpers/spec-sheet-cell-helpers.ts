import { PUSpecSheetTableCell } from '../models/cells/pu-spec-sheet-table-cell';
import { PUSpecSheetTableCellBase } from '../models/cells/pu-spec-sheet-table-cell-base';
import { PUSpecSheetTableRow } from '../models/pu-spec-sheet-table-row';
import { CellTypeRegistry } from '../models/services/cell-type-registry';
import { SheetTableCell } from '../../../core/models/domain/sheets/sheet-table-cell';

export class SpecSheetCellHelpers {
  public static toCell(
    cell: SheetTableCell,
    parent: PUSpecSheetTableRow,
  ): PUSpecSheetTableCellBase {
    const type = cell.value.$type;
    const propertiesType = cell.configurationOverride
      ? cell.configurationOverride.$type
      : cell.cellConfiguration!.properties.$type;

    const switchType = type !== propertiesType ? propertiesType : type;

    return (
      CellTypeRegistry.createCell(switchType, cell, parent) ??
      new PUSpecSheetTableCell(cell, parent)
    );
  }
}
