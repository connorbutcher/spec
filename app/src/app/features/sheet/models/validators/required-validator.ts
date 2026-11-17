import { computed } from '@angular/core';
import { isNil } from 'lodash';
import { Validator } from '../../../../core/models/domain/configuration/validator';
import { PUSpecSheetTableCell } from '../cells/pu-spec-sheet-table-cell';
import { ValidatorBase } from './validator-base';

export class RequiredValidator extends ValidatorBase {
  public override readonly isValid = computed(() => !isNil(this._cell.value()));
  public override readonly message = computed(() => {
    return `${this._cell.tableCellType().title} - This field is required.`;
  });

  private _cell: PUSpecSheetTableCell;

  constructor(validator: Validator, cells: PUSpecSheetTableCell[]) {
    super(validator, cells);

    this._cell = cells[0];
  }
}
