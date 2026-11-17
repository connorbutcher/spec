import { CellTypeReferenceKey } from '../../../../../core/models/domain/enums/cell-type-reference-key';
import { LinkedSpecSheetDropdownCellProperties } from '../components/linked-spec-sheet-dropdown-cell-properties/linked-spec-sheet-dropdown-cell-properties';
import { NumberDropdownCellProperties } from '../components/number-dropdown-cell-properties/number-dropdown-cell-properties';
import { TableValuesDropdownCellProperties } from '../components/table-values-dropdown-cell-properties/table-values-dropdown-cell-properties';
import { TextDropdownCellProperties } from '../components/text-dropdown-cell-properties/text-dropdown-cell-properties';

export class CellPropertiesComponentHelper {
  private static readonly _cellTypeMap = new Map<CellTypeReferenceKey, any>([
    [
      CellTypeReferenceKey.TableValuesDropdown,
      TableValuesDropdownCellProperties,
    ],
    [CellTypeReferenceKey.TextDropdown, TextDropdownCellProperties],
    [CellTypeReferenceKey.NumericDropdown, NumberDropdownCellProperties],
    [
      CellTypeReferenceKey.LinkedSheetTableValuesDropdown,
      LinkedSpecSheetDropdownCellProperties,
    ],
    [
      CellTypeReferenceKey.EngineSpecPartDropdown,
      LinkedSpecSheetDropdownCellProperties,
    ],
  ]);

  public static getCellTypeComponent(cellType: CellTypeReferenceKey): any {
    return this._cellTypeMap.get(cellType);
  }
}
