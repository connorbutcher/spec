import { Signal } from '@angular/core';
import { Validator } from '../../../../core/models/domain/configuration/validator';
import { ValidatorProperty } from '../../../../core/models/domain/configuration/validator-properties/validator-property';
import { ValidatorLevel } from '../../../../core/types/configuration/validator-level.enum';
import { PUSpecSheetTableCellBase } from '../cells/pu-spec-sheet-table-cell-base';

export abstract class ValidatorBase<
  T extends ValidatorProperty | undefined = undefined,
> {
  public abstract isValid: Signal<boolean>;
  public abstract message: Signal<string>;

  constructor(
    protected readonly validator: Validator,
    protected readonly cells: PUSpecSheetTableCellBase[],
  ) {}

  public get level(): ValidatorLevel {
    return this.validator.level;
  }

  protected get property(): T {
    return this.validator.property as T;
  }
}
