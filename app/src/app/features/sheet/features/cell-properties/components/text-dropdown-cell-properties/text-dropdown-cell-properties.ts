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
import { TextDropdownCellProperty } from '../../models/text-dropdown-cell-property';
import { TextDropdownCellOptionsConfigurationControl } from './text-dropdown-cell-options-configuration-control/text-dropdown-cell-options-configuration-control';

export interface IDropdownCellTypeConfigurationForm {
  options: FormControl<string[]>;
  defaultValue: FormControl<string | undefined>;
}

@Component({
  selector: 'app-text-dropdown-cell-properties',
  imports: [
    ElementLabel,
    Select,
    FormsModule,
    ReactiveFormsModule,
    TextDropdownCellOptionsConfigurationControl,
  ],
  templateUrl: './text-dropdown-cell-properties.html',
  styleUrl: './text-dropdown-cell-properties.scss',
})
export class TextDropdownCellProperties extends Destroyable implements OnInit {
  public readonly cellProperty = input.required<TextDropdownCellProperty>();

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
