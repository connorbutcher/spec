import { ValidatorFn, Validators } from '@angular/forms';
import { MinMaxLengthValidatorProperty } from '../../../core/models/domain/configuration/validator-properties/min-max-length-validator-property';
import { MinMaxNumberValidatorProperty } from '../../../core/models/domain/configuration/validator-properties/min-max-number-validator-property';
import { ValidatorProperty } from '../../../core/models/domain/configuration/validator-properties/validator-property';
import { PropertyValidatorType } from '../../../core/types/properties/property-validator-type.enum';

export class PropertyValidatorHelpers {
  public static getValidator(
    validator: PropertyValidatorType,
    property?: ValidatorProperty,
  ): ValidatorFn {
    switch (validator) {
      case PropertyValidatorType.Required:
        return Validators.required;

      case PropertyValidatorType.MaxLength:
        if (property === undefined || property.$type !== 'minMaxLength') {
          throw new Error(
            'Property is required for MaxLength validator and must be of type minMaxLength',
          );
        }

        return Validators.maxLength(
          (property as MinMaxLengthValidatorProperty).number,
        );
      case PropertyValidatorType.MinLength:
        if (property === undefined || property.$type !== 'minMaxLength') {
          throw new Error(
            'Property is required for MinLength validator and must be of type minMaxLength',
          );
        }
        return Validators.minLength(
          (property as MinMaxLengthValidatorProperty).number,
        );
      case PropertyValidatorType.MaxNumber:
        if (property === undefined || property.$type !== 'minMaxNumber') {
          throw new Error(
            'Property is required for MaxNumber validator and must be of type minMaxNumber',
          );
        }
        return Validators.max(
          (property as MinMaxNumberValidatorProperty).number,
        );

      case PropertyValidatorType.MinNumber:
        if (property === undefined || property.$type !== 'minMaxNumber') {
          throw new Error(
            'Property is required for MinNumber validator and must be of type minMaxNumber',
          );
        }
        return Validators.min(
          (property as MinMaxNumberValidatorProperty).number,
        );

      default:
        return Validators.nullValidator;
    }
  }
}
