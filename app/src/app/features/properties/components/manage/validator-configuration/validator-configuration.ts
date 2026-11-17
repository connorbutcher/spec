import {
  Component,
  forwardRef,
  output,
  signal,
  viewChild,
  ViewContainerRef,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  ControlValueAccessor,
  FormControl,
  FormGroup,
  FormsModule,
  NG_VALIDATORS,
  NG_VALUE_ACCESSOR,
  ReactiveFormsModule,
  ValidationErrors,
  Validator,
  Validators,
} from '@angular/forms';
import { ButtonDirective } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { filter, takeUntil } from 'rxjs';
import { ValidatorProperty } from '../../../../../core/models/domain/configuration/validator-properties/validator-property';
import { PropertyValidatorType } from '../../../../../core/types/properties/property-validator-type.enum';
import { Destroyable } from '../../../../common/destructable/destructable';
import { PropertyValidator } from '../../../models/property-validator.class';
import { PropertyValidatorForm } from '../edit-property/edit-property';
import { ValidatorTypeHelpers } from './helpers/validator-type-helpers';

@Component({
  selector: 'app-validator-configuration',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    InputText,
    Select,
    ButtonDirective,
  ],
  templateUrl: './validator-configuration.html',
  styleUrl: './validator-configuration.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ValidatorConfigurationComponent),
      multi: true,
    },
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => ValidatorConfigurationComponent),
      multi: true,
    },
  ],
})
export class ValidatorConfigurationComponent
  extends Destroyable
  implements ControlValueAccessor, Validator
{
  public readonly extraPropertiesRef = viewChild('extraProperties', {
    read: ViewContainerRef,
  });

  public get value(): PropertyValidator | undefined {
    if (this.validatorForm.invalid) {
      return undefined;
    }

    this._value?.message.set(this.validatorForm.get('message')?.value!);
    this._value?.validator.set(this.validatorForm.get('validator')?.value!);
    this._value?.properties.set(this.validatorForm.get('properties')?.value!);
    return this._value;
  }
  public set value(value: PropertyValidator | undefined) {
    this._value = value;
    this.validatorForm.patchValue({
      validator: value?.validator(),
      message: value?.message(),
      properties: value?.properties(),
    });
  }

  public readonly deleteValidator = output<void>();
  public readonly validatorTypeValues = Object.values(PropertyValidatorType);
  public readonly validatorForm = new FormGroup<PropertyValidatorForm>({
    validator: new FormControl<PropertyValidatorType>(undefined!, {
      validators: [Validators.required],
      nonNullable: true,
    }),
    message: new FormControl<string>(undefined!, {
      validators: [Validators.required],
      nonNullable: true,
    }),
    properties: new FormControl<ValidatorProperty | undefined>(undefined, {
      nonNullable: true,
    }),
  });
  public readonly isDisabled = signal<boolean>(false);

  private _value: PropertyValidator | undefined = undefined;

  constructor() {
    super();

    this.validatorForm.valueChanges
      .pipe(
        takeUntilDestroyed(),
        filter(
          (value) =>
            this.value?.message() !== value.message ||
            this.value?.validator() !== value.validator ||
            this.value?.properties() !== value.properties,
        ),
      )
      .subscribe((value) => {
        this.value?.validator.set(value.validator!);
        this.value?.message.set(value.message!);
        this.value?.properties.set(value.properties!);
      });

    this.validatorCtrl.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((value) => {
        if (!value) {
          return;
        }

        const extraPropertiesRef = this.extraPropertiesRef();
        if (!extraPropertiesRef) {
          return;
        }

        extraPropertiesRef.clear();

        const validatorTypeComponent =
          ValidatorTypeHelpers.getValidatorType(value);
        if (validatorTypeComponent) {
          this.validatorCtrl.addValidators(Validators.required);
          this.validatorCtrl.updateValueAndValidity({ emitEvent: false });
          const componentRef = extraPropertiesRef.createComponent(
            validatorTypeComponent,
          );
          componentRef.setInput('control', this.propertiesCtrl);
        } else {
          this.validatorCtrl.removeValidators(Validators.required);
          this.validatorCtrl.updateValueAndValidity({ emitEvent: false });
        }
      });
  }

  public writeValue(validator: PropertyValidator): void {
    this.value = validator;
  }

  public registerOnChange(fn: (value: PropertyValidator) => void): void {
    this.validatorForm.valueChanges
      .pipe(takeUntil(this.destroy))
      .subscribe(() => fn(this.value!));
  }

  public registerOnTouched(fn: () => void): void {
    this._onTouched = fn;
  }

  public validate(control: AbstractControl): ValidationErrors | null {
    return this.validatorForm.valid ? null : { invalidValidator: true };
  }

  public setDisabledState?(isDisabled: boolean): void {
    this.isDisabled.set(isDisabled);

    if (isDisabled) {
      this.validatorForm.disable();
    } else {
      this.validatorForm.enable();
    }
  }

  private get validatorCtrl(): FormControl<PropertyValidatorType> {
    return this.validatorForm.get(
      'validator',
    ) as FormControl<PropertyValidatorType>;
  }

  private get propertiesCtrl(): FormControl<ValidatorProperty> {
    return this.validatorForm.get(
      'properties',
    ) as FormControl<ValidatorProperty>;
  }

  private _onChange: (value: PropertyValidator) => void = () => {};
  private _onTouched: () => void = () => {};
}
