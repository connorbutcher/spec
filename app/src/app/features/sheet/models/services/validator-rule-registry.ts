import { Validator } from '../../../../core/models/domain/configuration/validator';
import { ValidatorProperty } from '../../../../core/models/domain/configuration/validator-properties/validator-property';
import { PUSpecSheetTableCellBase } from '../cells/pu-spec-sheet-table-cell-base';
import { AllRequiredCellsValidator } from '../validators/all-required-cells-validator';
import { AnyRequiredCellsValidator } from '../validators/any-required-cells-validator';
import { CellValueOrderValidator } from '../validators/cell-value-order-validator';
import { RequiredValidator } from '../validators/required-validator';
import { ValidatorBase } from '../validators/validator-base';

export type ValidatorFactory = (
  validator: Validator,
  cells: PUSpecSheetTableCellBase[],
) => ValidatorBase<ValidatorProperty | undefined>;

export class ValidatorRuleRegistry {
  private static readonly _registry = new Map<string, ValidatorFactory>();

  static {
    ValidatorRuleRegistry.registerDefaults();
  }

  /**
   * Register a validator factory
   */
  public static register(type: string, factory: ValidatorFactory): void {
    this._registry.set(type, factory);
  }

  /**
   * Create a validator instance from the registry
   */
  public static createValidator(
    type: string,
    validator: Validator,
    cells: PUSpecSheetTableCellBase[],
  ): ValidatorBase<ValidatorProperty | undefined> | undefined {
    const factory = this._registry.get(type);
    if (!factory) {
      return undefined;
    }

    return factory(validator, cells);
  }

  /**
   * Register default validators
   */
  private static registerDefaults(): void {
    this.register(
      'required',
      (validator, cells) => new RequiredValidator(validator, cells),
    );
    this.register(
      'allCellsRequired',
      (validator, cells) => new AllRequiredCellsValidator(validator, cells),
    );
    this.register(
      'anyCellsRequired',
      (validator, cells) => new AnyRequiredCellsValidator(validator, cells),
    );
    this.register(
      'cellValueOrder',
      (validator, cells) => new CellValueOrderValidator(validator, cells),
    );
  }
}
