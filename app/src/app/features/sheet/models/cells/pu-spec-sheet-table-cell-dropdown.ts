import { computed, linkedSignal } from '@angular/core';
import { CellValueHelpers } from '../../../../core/helpers/cell-value-helpers';
import { UpdateHelpers } from '../../../../core/helpers/update-helpers';
import { NumericValue } from '../../../../core/models/domain/values/numeric-value';
import { TextValue } from '../../../../core/models/domain/values/text-value';
import { NumberDropdownCellProperty } from '../../features/cell-properties/models/number-dropdown-cell-property';
import { TextDropdownCellProperty } from '../../features/cell-properties/models/text-dropdown-cell-property';
import { PUSpecSheetTableRow } from '../pu-spec-sheet-table-row';
import { PUSpecSheetTableCellBase } from './pu-spec-sheet-table-cell-base';
import { SheetTableCell } from '../../../../core/models/domain/sheets/sheet-table-cell';

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

  constructor(cell: SheetTableCell, parent: PUSpecSheetTableRow) {
    super(cell, parent);
  }

  public override get originalValue(): any {
    return this.cellValue.value;
  }
}
