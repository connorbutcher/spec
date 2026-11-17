import { PropertyValidatorType } from '../../../../../../core/types/properties/property-validator-type.enum';
import { MinMaxLengthValidatorProperties } from '../../validator-properties/min-max-length-validator-properties/min-max-length-validator-properties';
import { MinMaxNumberValidatorProperties } from '../../validator-properties/min-max-number-validator-properties/min-max-number-validator-properties';

export class ValidatorTypeHelpers {
  private static readonly validatorTypeMap = new Map<
    PropertyValidatorType,
    any
  >([
    [PropertyValidatorType.MinLength, MinMaxLengthValidatorProperties],
    [PropertyValidatorType.MaxLength, MinMaxLengthValidatorProperties],
    [PropertyValidatorType.MinNumber, MinMaxNumberValidatorProperties],
    [PropertyValidatorType.MaxNumber, MinMaxNumberValidatorProperties],
  ]);

  public static getValidatorType(validatorType: PropertyValidatorType): any {
    return this.validatorTypeMap.get(validatorType) ?? null;
  }
}
