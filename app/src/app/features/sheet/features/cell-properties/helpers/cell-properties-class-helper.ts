import { CellTypeReferenceKey } from '../../../../../core/models/domain/enums/cell-type-reference-key';
import { CellProperties } from '../../../../../core/models/domain/properties/cell/cell-properties';
import { LinkedSpecSheetDropdownCellProperties } from '../../../../../core/models/domain/properties/cell/linked-spec-sheet-dropdown-cell-properties';
import { NumberDropdownCellProperties } from '../../../../../core/models/domain/properties/cell/number-dropdown-cell-properties';
import { TextDropdownCellProperties } from '../../../../../core/models/domain/properties/cell/text-dropdown-cell-properties';
import { TableCellTemplate } from '../../../../../core/models/domain/template/table-cell-template';
import { TableCellType } from '../../../../../core/models/domain/template/table-cell-type';
import { CellPropertyBase } from '../models/cell-property-base';
import { LinkedSpecSheetValueDropdownPhaseTypeCellProperty } from '../models/linked-spec-sheet-value-dropdown-phase-type-cell-property';
import { NumberDropdownCellProperty } from '../models/number-dropdown-cell-property';
import { TableValuesDropdownCellProperty } from '../models/table-values-dropdown-cell-property';
import { TextDropdownCellProperty } from '../models/text-dropdown-cell-property';

export class CellPropertiesClassHelpers {
  public static mapToClassFromConfigure(
    cellTemplate: TableCellTemplate,
  ): CellPropertyBase<CellProperties> {
    switch (cellTemplate.tableCellType!.key) {
      case CellTypeReferenceKey.TableValuesDropdown:
        return new TableValuesDropdownCellProperty(
          cellTemplate.tableCellType!,
          cellTemplate.cellConfiguration!.properties,
          cellTemplate,
        );
      case CellTypeReferenceKey.TextDropdown:
        return new TextDropdownCellProperty(
          cellTemplate.tableCellType!,
          cellTemplate.cellConfiguration!
            .properties as TextDropdownCellProperties,
          cellTemplate,
        );
      case CellTypeReferenceKey.NumericDropdown:
        return new NumberDropdownCellProperty(
          cellTemplate.tableCellType!,
          cellTemplate.cellConfiguration!
            .properties as NumberDropdownCellProperties,
          cellTemplate,
        );
      case CellTypeReferenceKey.LinkedSheetTableValuesDropdown:
        return new LinkedSpecSheetValueDropdownPhaseTypeCellProperty(
          cellTemplate.tableCellType!,
          cellTemplate.cellConfiguration!
            .properties as LinkedSpecSheetDropdownCellProperties,
          cellTemplate,
        );
      case CellTypeReferenceKey.EngineSpecPartDropdown:
        return new LinkedSpecSheetValueDropdownPhaseTypeCellProperty(
          cellTemplate.tableCellType!,
          cellTemplate.cellConfiguration!
            .properties as LinkedSpecSheetDropdownCellProperties,
          cellTemplate,
        );
      default:
        return undefined!;
    }
  }

  public static mapToClassFromCell(
    tableCellType: TableCellType,
    properties: CellProperties,
    cellTemplate?: TableCellTemplate,
  ): CellPropertyBase<CellProperties> {
    switch (tableCellType!.key) {
      case CellTypeReferenceKey.TableValuesDropdown:
        return new TableValuesDropdownCellProperty(
          tableCellType!,
          properties,
          cellTemplate,
        );
      case CellTypeReferenceKey.TextDropdown:
        return new TextDropdownCellProperty(
          tableCellType!,
          properties as TextDropdownCellProperties,
          cellTemplate,
        );
      case CellTypeReferenceKey.NumericDropdown:
        return new NumberDropdownCellProperty(
          tableCellType!,
          properties as NumberDropdownCellProperties,
          cellTemplate,
        );
      case CellTypeReferenceKey.LinkedSheetTableValuesDropdown:
        return new LinkedSpecSheetValueDropdownPhaseTypeCellProperty(
          tableCellType!,
          properties as LinkedSpecSheetDropdownCellProperties,
          cellTemplate,
        );
      case CellTypeReferenceKey.EngineSpecPartDropdown:
        return new LinkedSpecSheetValueDropdownPhaseTypeCellProperty(
          tableCellType!,
          properties as LinkedSpecSheetDropdownCellProperties,
          cellTemplate,
        );
      default:
        return undefined!;
    }
  }
}
