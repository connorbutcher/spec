import { computed } from '@angular/core';
import { isNil } from 'lodash';
import { Validator } from '../../../../core/models/domain/configuration/validator';
import { CellValueOrderValidatorProperty } from '../../../../core/models/domain/configuration/validator-properties/cell-value-order-validator-property';
import { PUSpecSheetTableCell } from '../cells/pu-spec-sheet-table-cell';
import { ValidatorBase } from './validator-base';

export class CellValueOrderValidator extends ValidatorBase<CellValueOrderValidatorProperty> {
  public override readonly isValid = computed(() => {
    const cells = this._validatorCells;
    const order = this.property.order;
    const cellsWithValues = cells.filter((c) => !isNil(c.value()));

    let hasInvalidOrder = false;
    for (let i = 1; i < cellsWithValues.length; i++) {
      const prevValue = cellsWithValues[i - 1].value();
      const currValue = cellsWithValues[i].value();
      if (!isNil(prevValue) && !isNil(currValue)) {
        if (order === 'asc' ? prevValue > currValue : prevValue < currValue) {
          hasInvalidOrder = true;
          break;
        }
      }
    }

    return !hasInvalidOrder;
  });
  public override readonly message = computed(() => this.validator.message);

  private _validatorCells: PUSpecSheetTableCell[];

  constructor(validator: Validator, cells: PUSpecSheetTableCell[]) {
    super(validator, cells);

    this._validatorCells = cells.filter((c) =>
      this.property.cellKeys.includes(c.tableCellType().key),
    );
  }
}
