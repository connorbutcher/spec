import { LinkedSpecSheetDropdownCellProperties } from '../models/domain/properties/cell/linked-spec-sheet-dropdown-cell-properties';
import { NumberCellProperties } from '../models/domain/properties/cell/number-cell-properties';
import { PairCellProperties } from '../models/domain/properties/cell/pair-cell-properties';
import { TextCellProperties } from '../models/domain/properties/cell/text-cell-properties';
import { TableCellTemplate } from '../models/domain/template/table-cell-template';
import { CellValue } from '../models/domain/values/cell-value';
import { LinkedSpecSheetValue } from '../models/domain/values/linked-spec-sheet-value';
import { NumericValue } from '../models/domain/values/numeric-value';
import { PairValue } from '../models/domain/values/pair-value';
import { TextValue } from '../models/domain/values/text-value';
import { nextTempId } from './temp-id-counter';

export class CellValueHelpers {
  public static buildNewCellValue(cellTemplate: TableCellTemplate): CellValue {
    const cellType = cellTemplate.tableCellType!;
    if (!cellType) {
      throw new Error('GroupCell does not have an associated CellType.');
    }

    const configuration = cellTemplate.cellConfiguration;
    if (!configuration) {
      throw new Error(
        'CellTemplate does not have an associated CellConfiguration.',
      );
    }

    const cellProperties = configuration.properties;
    switch (cellType.modelType) {
      case 'text':
      case 'textDropdown':
        return {
          $type: 'text',
          id: nextTempId(),
          specSheetTableCellId: 0,
          value: (cellProperties as TextCellProperties).defaultValue,
        } as TextValue;
      case 'numeric':
      case 'numericDropdown':
        return {
          $type: 'numeric',
          id: nextTempId(),
          specSheetTableCellId: 0,
          value: (cellProperties as NumberCellProperties).defaultValue,
        } as NumericValue;
      case 'pair': {
        const pairProperties = cellProperties as PairCellProperties;

        return {
          $type: 'pair',
          id: nextTempId(),
          specSheetTableCellId: 0,
          valueX: pairProperties?.defaultValueX || null,
          valueY: pairProperties?.defaultValueY || null,
        } as PairValue;
      }
      case 'linkedSpecSheet':
        const linkedSpecSheetProperties =
          cellProperties as LinkedSpecSheetDropdownCellProperties;

        return {
          $type: 'linkedSpecSheet',
          id: nextTempId(),
          specSheetTableCellId: 0,
          linkedReferenceId: linkedSpecSheetProperties?.defaultValue,
          linkedSpecSheetTypeId:
            linkedSpecSheetProperties?.linkedSpecSheetTypeId,
          tableReferenceId: linkedSpecSheetProperties?.tableReferenceId,
          linkedSpecSheetPhaseId:
            linkedSpecSheetProperties?.linkedSpecSheetPhaseId,
        } as LinkedSpecSheetValue;
      default:
        throw new Error(`Unsupported cell model type: ${cellType.modelType}`);
    }
  }

  public static buildCellValue(
    cellValue: CellValue,
    value: any | undefined,
  ): CellValue {
    switch (cellValue.$type) {
      case 'text':
      case 'textDropdown':
        return {
          ...cellValue,
          value: value as string | null,
        } as TextValue;
      case 'numeric':
      case 'numericDropdown':
        return {
          ...cellValue,
          value: value as number | null,
        } as NumericValue;
      case 'pair':
        return {
          ...cellValue,
          valueX: value?.valueX || null,
          valueY: value?.valueY || null,
        } as PairValue;
      case 'linkedSpecSheet':
        return {
          ...cellValue,
          linkedReferenceId: value?.linkedReferenceId || null,
          linkedSpecSheetTypeId: value?.linkedSpecSheetTypeId,
          linkedSpecSheetPhaseId: value?.linkedSpecSheetPhaseId,
          tableReferenceId: value?.tableReferenceId,
        } as LinkedSpecSheetValue;
      default:
        throw new Error(`Unsupported cell value type: ${cellValue.$type}`);
    }
  }
}
