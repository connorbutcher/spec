import { Component, inject, input, OnInit } from '@angular/core';
import {
  FormControl,
  FormGroup,
  FormsModule,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Select } from 'primeng/select';
import { debounceTime, filter, takeUntil } from 'rxjs';
import { Destroyable } from '../../../../../common/destructable/destructable';
import { ElementLabel } from '../../../../../common/element-label/element-label';
import { NumberDropdownCellProperty } from '../../models/number-dropdown-cell-property';
import { NumberDropdownCellOptionsConfigurationControl } from './number-dropdown-cell-options-configuration-control/number-dropdown-cell-options-configuration-control';

export interface IDropdownCellTypeConfigurationForm {
  options: FormControl<number[]>;
  defaultValue: FormControl<number | undefined>;
}

@Component({
  selector: 'app-number-dropdown-cell-properties',
  imports: [
    ElementLabel,
    Select,
    FormsModule,
    ReactiveFormsModule,
    NumberDropdownCellOptionsConfigurationControl,
  ],
  templateUrl: './number-dropdown-cell-properties.html',
  styleUrl: './number-dropdown-cell-properties.scss',
})
export class NumberDropdownCellProperties
  extends Destroyable
  implements OnInit
{
  public readonly cellProperty = input.required<NumberDropdownCellProperty>();

  public form!: FormGroup<IDropdownCellTypeConfigurationForm>;

  private readonly _formBuilder = inject(NonNullableFormBuilder);

  constructor() {
    super();
  }

  ngOnInit(): void {
    this.form = this.buildForm();

    this.form.valueChanges
      .pipe(
        takeUntil(this.destroy),
        debounceTime(200),
        filter(() => this.form.valid),
        filter(
          () =>
            this.cellProperty().options() !== this.form.value.options ||
            this.cellProperty().defaultValue() !== this.form.value.defaultValue,
        ),
      )
      .subscribe((value) => {
        this.cellProperty().options.set(this.form.value.options ?? []);
        this.cellProperty().defaultValue.set(this.form.value.defaultValue);

        if (
          value.defaultValue !== undefined &&
          !value.options?.includes(value.defaultValue)
        ) {
          this.form.controls.defaultValue.setValue(undefined);
        }
      });
  }

  public hasError(
    controlName: keyof IDropdownCellTypeConfigurationForm,
    error: string,
  ): boolean {
    return this.form.controls[controlName].hasError(error);
  }

  private buildForm(): FormGroup<IDropdownCellTypeConfigurationForm> {
    const dropdownCellProperties = this.cellProperty();

    return this._formBuilder.group<IDropdownCellTypeConfigurationForm>({
      options: this._formBuilder.control(dropdownCellProperties.options(), [
        Validators.required,
        Validators.minLength(1),
        Validators.nullValidator,
      ]),
      defaultValue: this._formBuilder.control(
        dropdownCellProperties.defaultValue(),
      ),
    });
  }
}
