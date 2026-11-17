import { computed, signal } from '@angular/core';
import { cloneDeep, isEqual } from 'lodash';
import { ValidatorProperty } from '../../../core/models/domain/configuration/validator-properties/validator-property';
import { SpecSheetTypePropertyValidator } from '../../../core/models/domain/types/spec-sheet-type-property-validator';
import { PropertyValidatorType } from '../../../core/types/properties/property-validator-type.enum';

export class PropertyValidator {
  public readonly validator = signal<PropertyValidatorType>(undefined!);
  public readonly message = signal<string>('');
  public readonly properties = signal<ValidatorProperty | undefined>(undefined);

  public readonly isValid = computed<boolean>(
    () => this.validator() !== undefined && this.message() !== undefined,
  );
  public readonly hasChanges = computed<boolean>(
    () =>
      this.validator() !== this.templatePropertyValidator.validator ||
      this.message() !== this.templatePropertyValidator.message ||
      !isEqual(this.properties(), this.templatePropertyValidator.property),
  );

  constructor(
    public readonly templatePropertyValidator: SpecSheetTypePropertyValidator,
  ) {
    this.validator.set(templatePropertyValidator.validator);
    this.message.set(templatePropertyValidator.message);
    this.properties.set(cloneDeep(templatePropertyValidator.property));
  }

  public toTemplatePropertyValidator(): SpecSheetTypePropertyValidator {
    return {
      validator: this.validator(),
      message: this.message(),
      property: this.properties(),
    };
  }
}
