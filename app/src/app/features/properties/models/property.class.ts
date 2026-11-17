import { computed, signal } from '@angular/core';
import { UpdateHelpers } from '../../../core/helpers/update-helpers';
import { SpecSheetTypeProperty } from '../../../core/models/domain/types/spec-sheet-type-property';
import { CreateSpecSheetTypePropertyRequest } from '../../../core/models/requests/types/properties/create-spec-sheet-type-property-request';
import { UpdateSpecSheetTypePropertyRequest } from '../../../core/models/requests/types/properties/update-spec-sheet-type-property-request';
import { PropertyControlType } from '../../../core/types/properties/property-control-type.enum';
import { PropertyValueType } from '../../../core/types/properties/property-value-type.enum';
import { PropertyValidator } from './property-validator.class';

export class Property {
  public readonly id: number;
  public readonly referenceId: string;
  public readonly valueName = signal<string>('');
  public readonly valueKey = signal<string>('');
  public readonly valueType = signal<PropertyValueType>(undefined!);
  public readonly valueControlType = signal<PropertyControlType>(undefined!);
  public readonly canValueBeSetInForm = signal<boolean>(false);
  public readonly displayOrder = signal<number>(0);
  public readonly placeholderText = signal<string | undefined>(undefined);
  public readonly validators = signal<PropertyValidator[]>([]);
  public readonly canBeEdited = signal<boolean>(true);
  public readonly canBeRemoved = signal<boolean>(true);

  public readonly isValid = computed(() => {
    return (
      this.valueName()?.length > 0 &&
      this.valueKey()?.length > 0 &&
      this.valueType()?.length > 0 &&
      this.valueControlType()?.length > 0 &&
      this.validators().every((x) => x.isValid())
    );
  });
  public readonly hasChanges = computed(() => {
    return (
      this.valueName() !== this.property.valueName ||
      this.valueKey() !== this.property.valueKey ||
      this.valueType() !== this.property.valueType ||
      this.valueControlType() !== this.property.valueControlType ||
      this.canValueBeSetInForm() !== this.property.canValueBeSetInForm ||
      this.placeholderText() !== this.property.placeholderText ||
      this.displayOrder() !== this.property.displayOrder ||
      this.validators().some((x) => x.hasChanges())
    );
  });

  constructor(public readonly property: SpecSheetTypeProperty) {
    this.id = property.id;
    this.referenceId = property.referenceId;
    this.valueName.set(property.valueName);
    this.valueKey.set(property.valueKey);
    this.valueType.set(property.valueType as PropertyValueType);
    this.valueControlType.set(property.valueControlType as PropertyControlType);
    this.canValueBeSetInForm.set(property.canValueBeSetInForm);
    this.displayOrder.set(property.displayOrder);
    this.placeholderText.set(property.placeholderText);
    this.validators.set(
      property.validators?.map(
        (validator) => new PropertyValidator(validator),
      ) ?? [],
    );
    this.canBeRemoved.set(property.saveValueAsProperty);
    this.canBeEdited.set(property.saveValueAsProperty);
  }

  public toProperty(): SpecSheetTypeProperty {
    return {
      id: this.id,
      referenceId: this.referenceId,
      specSheetTypeId: this.property.specSheetTypeId,
      valueName: this.valueName(),
      valueKey: this.valueKey(),
      valueType: this.valueType(),
      valueControlType: this.valueControlType(),
      canValueBeSetInForm: this.canValueBeSetInForm(),
      displayOrder: this.displayOrder(),
      placeholderText: this.placeholderText(),
      validators: this.validators().map((validator) =>
        validator.toTemplatePropertyValidator(),
      ),
      saveValueAsProperty: this.property.saveValueAsProperty,
    };
  }

  public toNewPropertyRequest(): CreateSpecSheetTypePropertyRequest {
    return {
      specSheetTypeId: this.property.specSheetTypeId,
      referenceId: this.referenceId,
      valueName: this.valueName(),
      valueKey: this.valueKey(),
      valueType: this.valueType(),
      valueControlType: this.valueControlType(),
      canValueBeSetInForm: this.canValueBeSetInForm(),
      displayOrder: this.displayOrder(),
      placeholderText: this.placeholderText(),
      validators: this.validators().map((validator) =>
        validator.toTemplatePropertyValidator(),
      ),
    };
  }

  public toUpdatePropertyRequest(): UpdateSpecSheetTypePropertyRequest {
    return {
      id: this.id,
      valueName: UpdateHelpers.compare(
        this.property.valueName,
        this.valueName(),
      ),
      valueKey: UpdateHelpers.compare(this.property.valueKey, this.valueKey()),
      valueType: UpdateHelpers.compare(
        this.property.valueType,
        this.valueType(),
      ),
      valueControlType: UpdateHelpers.compare(
        this.property.valueControlType,
        this.valueControlType(),
      ),
      canValueBeSetInForm: UpdateHelpers.compare(
        this.property.canValueBeSetInForm,
        this.canValueBeSetInForm(),
      ),
      displayOrder: UpdateHelpers.compare(
        this.property.displayOrder,
        this.displayOrder(),
      ),
      placeholderText: UpdateHelpers.compareToPropertyChanged(
        this.property.placeholderText,
        this.placeholderText(),
      ),
      validators: UpdateHelpers.compareToPropertyChanged(
        this.property.validators,
        this.validators().map((validator) =>
          validator.toTemplatePropertyValidator(),
        ),
      ),
    };
  }

  public get isNew(): boolean {
    return this.id <= 0;
  }
}
