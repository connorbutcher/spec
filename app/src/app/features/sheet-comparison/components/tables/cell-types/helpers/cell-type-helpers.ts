import { CellTypeReferenceKey } from '../../../../../../core/models/domain/enums/cell-type-reference-key';
import { EngineSpecDropdownCellComparison } from '../engine-spec-dropdown-cell-comparison/engine-spec-dropdown-cell-comparison';
import { NumberCellComparison } from '../number-cell-comparison/number-cell-comparison';
import { TableDropdownCellComparison } from '../table-dropdown-cell-comparison/table-dropdown-cell-comparison';
import { TextCellComparison } from '../text-cell-comparison/text-cell-comparison';
import { TextPairCellComparison } from '../text-pair-cell-comparison/text-pair-cell-comparison';

export class CellTypeHelpers {
  private static readonly _cellTypeMap = new Map<CellTypeReferenceKey, any>([
    [CellTypeReferenceKey.Header, TextCellComparison],
    [CellTypeReferenceKey.Group, TextCellComparison],
    [CellTypeReferenceKey.Description, TextCellComparison],
    [CellTypeReferenceKey.MinNoOk, NumberCellComparison],
    [CellTypeReferenceKey.Min, NumberCellComparison],
    [CellTypeReferenceKey.Max, NumberCellComparison],
    [CellTypeReferenceKey.MaxNoOk, NumberCellComparison],
    [CellTypeReferenceKey.NumericValue, NumberCellComparison],
    [CellTypeReferenceKey.TextValue, TextCellComparison],
    [CellTypeReferenceKey.PFKPartNumber, TextCellComparison],
    [CellTypeReferenceKey.PartNumber, TextCellComparison],
    [CellTypeReferenceKey.PartsPartNumber, TextCellComparison],
    [CellTypeReferenceKey.PartNumberUpper, TextCellComparison],
    [CellTypeReferenceKey.PartNumberLower, TextCellComparison],
    [CellTypeReferenceKey.Nom, NumberCellComparison],
    [CellTypeReferenceKey.GradeNumeric, NumberCellComparison],
    [CellTypeReferenceKey.GradeText, TextCellComparison],
    [CellTypeReferenceKey.EnginePhase, TextCellComparison],
    [CellTypeReferenceKey.TableValuesDropdown, TableDropdownCellComparison],
    [CellTypeReferenceKey.TextDropdown, TextCellComparison],
    [CellTypeReferenceKey.NumericDropdown, NumberCellComparison],
    [
      CellTypeReferenceKey.LinkedSheetTableValuesDropdown,
      TableDropdownCellComparison,
    ],
    [CellTypeReferenceKey.PartNumberPair, TextPairCellComparison],
    [
      CellTypeReferenceKey.EngineSpecPartDropdown,
      EngineSpecDropdownCellComparison,
    ],
    [CellTypeReferenceKey.EndGap, NumberCellComparison],
    [CellTypeReferenceKey.Thickness, NumberCellComparison],
  ]);

  public static getCellTypeComponent(cellType: CellTypeReferenceKey): any {
    return this._cellTypeMap.get(cellType);
  }
}
