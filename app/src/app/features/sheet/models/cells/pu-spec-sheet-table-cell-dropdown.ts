import { computed, linkedSignal } from '@angular/core';
import { CellValueHelpers } from '../../../../core/helpers/cell-value-helpers';
import { UpdateHelpers } from '../../../../core/helpers/update-helpers';
import { SpecSheetTableCell } from '../../../../core/models/domain/spec-sheet/spec-sheet-table-cell';
import { NumericValue } from '../../../../core/models/domain/values/numeric-value';
import { TextValue } from '../../../../core/models/domain/values/text-value';
import { CreateSpecSheetTableCellRequest } from '../../../../core/models/requests/spec-sheets/cells/create-spec-sheet-table-cell-request';
import { UpdateSpecSheetTableCellRequest } from '../../../../core/models/requests/spec-sheets/cells/update-spec-sheet-table-cell-request';
import { NumberDropdownCellProperty } from '../../features/cell-properties/models/number-dropdown-cell-property';
import { TextDropdownCellProperty } from '../../features/cell-properties/models/text-dropdown-cell-property';
import { PUSpecSheetTableRow } from '../pu-spec-sheet-table-row';
import { PUSpecSheetTableCellBase } from './pu-spec-sheet-table-cell-base';

export class PUSpecSheetTableCellDropdown extends PUSpecSheetTableCellBase<
  TextValue | NumericValue,
  TextDropdownCellProperty | NumberDropdownCellProperty
> {
  public override readonly value = linkedSignal(() => {
    const cellValue = this.cellValue;
    const dropdownOptions = this.properties()?.options();
    if (!cellValue) {
      return null;
    }

    const value = (cellValue as any).value;
    const matchedOption = dropdownOptions?.find((option) => option === value);
    return matchedOption ?? null;
  });
  public override readonly hasChanges = computed(() => {
    if (!this.cell) {
      return true;
    }

    return (
      this._changeTracker.hasChanges() ||
      this.properties()?.hasChanges() ||
      this.originalValue !== this.value()
    );
  });

  constructor(cell: SpecSheetTableCell, parent: PUSpecSheetTableRow) {
    super(cell, parent);
  }

  public override toNewTableCellRequest(): CreateSpecSheetTableCellRequest {
    return {
      specSheetTableGroupId: this.cell.specSheetTableRowId,
      specSheetTableRowId: this.cell.specSheetTableRowId,
      tableCellTypeId: this.cell.tableCellTypeId,
      cellConfigurationId: this.cell.cellConfigurationId,
      referenceId: this.referenceId(),
      rowNumber: this.rowNumber(),
      columnNumber: this.columnNumber(),
      colSpan: this.colSpan(),
      rowSpan: this.rowSpan(),
      value: CellValueHelpers.buildCellValue(this.cellValue, this.value()),
      configurationOverride: this.cell.configurationOverride,
    };
  }

  public override toUpdateTableCellRequest(): UpdateSpecSheetTableCellRequest {
    return {
      id: this.id,
      rowNumber: UpdateHelpers.compare(this.cell.rowNumber, this.rowNumber()),
      columnNumber: UpdateHelpers.compare(
        this.cell.columnNumber,
        this.columnNumber(),
      ),
      configurationOverride: UpdateHelpers.compareToPropertyChanged(
        this.cell.configurationOverride,
        this.properties()?.toCellConfiguration(),
      ),
      cellValue: UpdateHelpers.compareToPropertyChanged(this.cellValue, {
        $type: this.cellValue.$type,
        id: this.cellValue.id,
        specSheetTableCellId: this.cellValue.specSheetTableCellId,
        value: this.value(),
      } as TextValue | NumericValue),
    };
  }

  public override get originalValue(): any {
    return this.cellValue.value;
  }
}
