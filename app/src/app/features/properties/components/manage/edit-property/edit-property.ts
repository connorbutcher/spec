import { Component, effect, input, untracked } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormArray,
  FormControl,
  FormGroup,
  FormsModule,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ButtonDirective } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { filter } from 'rxjs';
import { ValidatorProperty } from '../../../../../core/models/domain/configuration/validator-properties/validator-property';
import { PropertyControlType } from '../../../../../core/types/properties/property-control-type.enum';
import { PropertyValidatorType } from '../../../../../core/types/properties/property-validator-type.enum';
import { PropertyValueType } from '../../../../../core/types/properties/property-value-type.enum';
import { ElementLabel } from '../../../../common/element-label/element-label';
import { PropertyValidator } from '../../../models/property-validator.class';
import { Property } from '../../../models/property.class';
import { ValidatorConfigurationComponent } from '../validator-configuration/validator-configuration';

interface EditPropertyForm {
  valueName: FormControl<string>;
  valueKey: FormControl<string>;
  valueType: FormControl<PropertyValueType>;
  valueControlType: FormControl<PropertyControlType>;
  canValueBeSetInForm: FormControl<boolean>;
  placeholderText: FormControl<string | undefined>;
  validators: FormArray<FormControl<PropertyValidator>>;
}

export interface PropertyValidatorForm {
  validator: FormControl<PropertyValidatorType>;
  message: FormControl<string>;
  properties: FormControl<ValidatorProperty | undefined>;
}

@Component({
  selector: 'app-edit-property',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    ElementLabel,
    InputText,
    Select,
    ButtonDirective,
    ValidatorConfigurationComponent,
  ],
  templateUrl: './edit-property.html',
  styleUrl: './edit-property.scss',
})
export class EditProperty {
  public readonly editingProperty = input.required<Property>();
  public readonly editPropertyForm: FormGroup<EditPropertyForm>;
  public readonly selectButtonOptions = [
    { label: 'Yes', value: true },
    { label: 'No', value: false },
  ];
  public readonly propertyValueTypeValues = Object.values(
    PropertyValueType,
  ).sort((a, b) => a.localeCompare(b));
  public readonly propertyControlTypeValues = Object.values(
    PropertyControlType,
  ).sort((a, b) => a.localeCompare(b));

  constructor(private readonly _formBuilder: NonNullableFormBuilder) {
    this.editPropertyForm = this.buildForm();

    effect(() => {
      const editingProperty = this.editingProperty()!;
      this.editPropertyForm.patchValue({
        valueName: untracked(() => editingProperty.valueName()),
        valueKey: untracked(() => editingProperty.valueKey()),
        valueControlType: untracked(
          () => editingProperty.valueControlType() as PropertyControlType,
        ),
        valueType: untracked(
          () => editingProperty.valueType() as PropertyValueType,
        ),
        canValueBeSetInForm: untracked(() =>
          editingProperty.canValueBeSetInForm(),
        ),
        placeholderText: untracked(() => editingProperty.placeholderText()),
      });

      this.validatorsCtrls.clear();
      for (const validator of untracked(() => editingProperty.validators())) {
        this.addValidator(validator);
      }

      editingProperty.canBeEdited()
        ? this.editPropertyForm.enable()
        : this.editPropertyForm.disable();
    });

    this.editPropertyForm.valueChanges
      .pipe(
        takeUntilDestroyed(),
        filter(
          (value) =>
            this.editPropertyForm.valid &&
            value.hasOwnProperty('valueName') &&
            (value.valueName !== this.editingProperty().valueName() ||
              value.valueKey !== this.editingProperty().valueKey() ||
              value.canValueBeSetInForm !==
                this.editingProperty().canValueBeSetInForm() ||
              value.valueControlType !==
                this.editingProperty().valueControlType() ||
              value.valueType !== this.editingProperty().valueType() ||
              value.placeholderText !==
                this.editingProperty().placeholderText() ||
              (!!value.validators
                ? value.validators !== this.editingProperty().validators()
                : false)),
        ),
      )
      .subscribe((value) => {
        if (!value) {
          return;
        }

        this.editingProperty().valueName.set(value.valueName!);
        this.editingProperty().valueKey.set(value.valueKey!);
        this.editingProperty().canValueBeSetInForm.set(
          value.canValueBeSetInForm!,
        );
        this.editingProperty().placeholderText.set(value.placeholderText);
        this.editingProperty().valueControlType.set(value.valueControlType!);
        this.editingProperty().valueType.set(value.valueType!);

        if (!this.editingProperty().canValueBeSetInForm()) {
          this.editingProperty().validators.set([]);
          this.validatorsCtrls.clear();
        }
      });
  }

  public addValidator(propertyValidator?: PropertyValidator): void {
    if (!propertyValidator) {
      propertyValidator = new PropertyValidator({
        message: undefined!,
        validator: undefined!,
      });
      this.editingProperty().validators.update((validators) => [
        ...validators,
        propertyValidator!,
      ]);
    }

    const validatorCtrl = this._formBuilder.control<PropertyValidator>(
      propertyValidator,
      [Validators.required],
    );
    this.validatorsCtrls.push(validatorCtrl);
  }

  public removeValidator(index: number): void {
    this.validatorsCtrls.removeAt(index);
    this.editingProperty().validators.update((validators) => {
      const newValidators = [...validators];
      newValidators.splice(index, 1);
      return newValidators;
    });
  }

  public hasError(
    controlName: keyof EditPropertyForm,
    errorName: string,
  ): boolean {
    const control = this.editPropertyForm.get(controlName);
    if (!control) {
      return true;
    }

    return control.hasError(errorName) && (control.dirty || control.touched);
  }

  public get valueControlTypeCtrl(): FormControl<PropertyControlType> {
    return this.editPropertyForm.get(
      'valueControlType',
    ) as FormControl<PropertyControlType>;
  }

  public get validatorsCtrls(): FormArray<FormControl<PropertyValidator>> {
    return this.editPropertyForm.get('validators') as FormArray<
      FormControl<PropertyValidator>
    >;
  }

  private buildForm(): FormGroup<EditPropertyForm> {
    return this._formBuilder.group({
      valueName: this._formBuilder.control<string>('', [
        Validators.required,
        Validators.minLength(1),
        Validators.maxLength(256),
      ]),
      valueKey: this._formBuilder.control<string>('', [
        Validators.required,
        Validators.minLength(1),
        Validators.maxLength(256),
      ]),
      valueType: this._formBuilder.control<PropertyValueType>(undefined!),
      valueControlType: this._formBuilder.control<PropertyControlType>(
        undefined!,
      ),
      canValueBeSetInForm: this._formBuilder.control<boolean>(true, [
        Validators.required,
      ]),
      placeholderText: this._formBuilder.control<string | undefined>(
        undefined,
        [Validators.maxLength(512)],
      ),
      validators: this._formBuilder.array<FormControl<PropertyValidator>>([]),
    });
  }
}
