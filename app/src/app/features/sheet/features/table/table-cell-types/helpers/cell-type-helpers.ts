import { CellTypeReferenceKey } from '../../../../../../core/models/domain/enums/cell-type-reference-key';
import { DisplayCellType } from '../display-cell-type/display-cell-type';
import { EnginePhaseCellType } from '../engine-phase-cell-type/engine-phase-cell-type';
import { EngineSpecPartDropdownCellType } from '../engine-spec-part-dropdown-cell-type/engine-spec-part-dropdown-cell-type';
import { GroupCellType } from '../group-cell-type/group-cell-type';
import { LinkedSpecSheetDropdownCellType } from '../linked-spec-sheet-dropdown-cell-type/linked-spec-sheet-dropdown-cell-type';
import { NumberCellType } from '../number-cell-type/number-cell-type';
import { NumberDropdownCellType } from '../number-dropdown-cell-type/number-dropdown-cell-type';
import { PartsPartNumberCellType } from '../parts-part-number-cell-type/parts-part-number-cell-type';
import { PartsPartNumberPairCellType } from '../parts-part-number-pair-cell-type/parts-part-number-pair-cell-type';
import { PFKPartNumberCellType } from '../pfk-part-number-cell-type/pfk-part-number-cell-type';
import { TableValuesDropdownCellType } from '../table-values-dropdown-cell-type/table-values-dropdown-cell-type';
import { TextCellType } from '../text-cell-type/text-cell-type';
import { TextDropdownCellType } from '../text-dropdown-cell-type/text-dropdown-cell-type';

export class CellTypeHelpers {
  private static readonly _cellTypeMap = new Map<CellTypeReferenceKey, any>([
    [CellTypeReferenceKey.Header, DisplayCellType],
    [CellTypeReferenceKey.Group, GroupCellType],
    [CellTypeReferenceKey.Description, TextCellType],
    [CellTypeReferenceKey.MinNoOk, NumberCellType],
    [CellTypeReferenceKey.Min, NumberCellType],
    [CellTypeReferenceKey.Max, NumberCellType],
    [CellTypeReferenceKey.MaxNoOk, NumberCellType],
    [CellTypeReferenceKey.NumericValue, NumberCellType],
    [CellTypeReferenceKey.TextValue, TextCellType],
    [CellTypeReferenceKey.PFKPartNumber, PFKPartNumberCellType],
    [CellTypeReferenceKey.PartNumber, TextCellType],
    [CellTypeReferenceKey.PartsPartNumber, PartsPartNumberCellType],
    [CellTypeReferenceKey.PartNumberUpper, TextCellType],
    [CellTypeReferenceKey.PartNumberLower, TextCellType],
    [CellTypeReferenceKey.Nom, NumberCellType],
    [CellTypeReferenceKey.GradeNumeric, NumberCellType],
    [CellTypeReferenceKey.GradeText, TextCellType],
    [CellTypeReferenceKey.EnginePhase, EnginePhaseCellType],
    [CellTypeReferenceKey.TableValuesDropdown, TableValuesDropdownCellType],
    [CellTypeReferenceKey.TextDropdown, TextDropdownCellType],
    [CellTypeReferenceKey.NumericDropdown, NumberDropdownCellType],
    [
      CellTypeReferenceKey.LinkedSheetTableValuesDropdown,
      LinkedSpecSheetDropdownCellType,
    ],
    [CellTypeReferenceKey.PartNumberPair, PartsPartNumberPairCellType],
    [
      CellTypeReferenceKey.EngineSpecPartDropdown,
      EngineSpecPartDropdownCellType,
    ],
    [CellTypeReferenceKey.EndGap, NumberCellType],
    [CellTypeReferenceKey.Thickness, NumberCellType],
  ]);

  public static getCellTypeComponent(cellType: CellTypeReferenceKey): any {
    return this._cellTypeMap.get(cellType);
  }
}
