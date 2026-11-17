import { computed } from '@angular/core';
import { isNil } from 'lodash';
import { Validator } from '../../../../core/models/domain/configuration/validator';
import { CellsRequiredValidatorProperty } from '../../../../core/models/domain/configuration/validator-properties/cells-required-validator-property';
import { PUSpecSheetTableCell } from '../cells/pu-spec-sheet-table-cell';
import { ValidatorBase } from './validator-base';

export class AllRequiredCellsValidator extends ValidatorBase<CellsRequiredValidatorProperty> {
  public override readonly isValid = computed(() => {
    const cells = this._validatorCells;
    return cells.every((c) => !isNil(c.value()));
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
