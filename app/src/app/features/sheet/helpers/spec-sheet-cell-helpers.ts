import { SpecSheetTableCell } from '../../../core/models/domain/spec-sheet/spec-sheet-table-cell';
import { PUSpecSheetTableCell } from '../models/cells/pu-spec-sheet-table-cell';
import { PUSpecSheetTableCellBase } from '../models/cells/pu-spec-sheet-table-cell-base';
import { PUSpecSheetTableRow } from '../models/pu-spec-sheet-table-row';
import { CellTypeRegistry } from '../models/services/cell-type-registry';

export class SpecSheetCellHelpers {
  public static toCell(
    cell: SpecSheetTableCell,
    parent: PUSpecSheetTableRow,
  ): PUSpecSheetTableCellBase {
    const type = cell.cellValue.$type;
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
