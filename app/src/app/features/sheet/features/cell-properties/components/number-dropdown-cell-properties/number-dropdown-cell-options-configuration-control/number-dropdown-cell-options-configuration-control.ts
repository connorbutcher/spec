import { CdkDragDrop, DragDropModule } from '@angular/cdk/drag-drop';
import { Component, forwardRef, Input, signal } from '@angular/core';
import {
  AbstractControl,
  ControlValueAccessor,
  FormArray,
  FormControl,
  NG_VALIDATORS,
  NG_VALUE_ACCESSOR,
  ReactiveFormsModule,
  ValidationErrors,
  Validator,
  Validators,
} from '@angular/forms';
import { ButtonDirective } from 'primeng/button';
import { InputNumber } from 'primeng/inputnumber';
import { takeUntil } from 'rxjs';
import { Destroyable } from '../../../../../../common/destructable/destructable';

@Component({
  selector: 'app-number-dropdown-cell-options-configuration-control',
  standalone: true,
  imports: [ReactiveFormsModule, InputNumber, ButtonDirective, DragDropModule],
  templateUrl: './number-dropdown-cell-options-configuration-control.html',
  styleUrl: './number-dropdown-cell-options-configuration-control.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(
        () => NumberDropdownCellOptionsConfigurationControl,
      ),
      multi: true,
    },
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(
        () => NumberDropdownCellOptionsConfigurationControl,
      ),
      multi: true,
    },
  ],
})
export class NumberDropdownCellOptionsConfigurationControl
  extends Destroyable
  implements ControlValueAccessor, Validator
{
  @Input()
  public get value(): number[] {
    return this.optionsFormArray.value;
  }
  public set value(values: number[]) {
    for (const value of values) {
      this.optionsFormArray.push(
        new FormControl(value, {
          validators: [Validators.required, Validators.nullValidator],
          nonNullable: true,
        }),
      );
    }
  }

  public readonly disabled = signal<boolean>(false);
  public readonly optionsFormArray = new FormArray<FormControl<number>>([]);

  writeValue(valueToWrite: number[]): void {
    this.value = valueToWrite;
  }

  registerOnChange(fn: any): void {
    this.optionsFormArray.valueChanges
      .pipe(takeUntil(this.destroy))
      .subscribe(() => fn(this.value));
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  validate(control: AbstractControl): ValidationErrors | null {
    return this.optionsFormArray.valid ? null : { invalid: true };
  }

  public drop(event: CdkDragDrop<FormArray<FormControl<number>>>) {
    const previousIndex = this.optionsFormArray.controls[event.previousIndex];
    this.optionsFormArray.controls[event.previousIndex] =
      this.optionsFormArray.controls[event.currentIndex];
    this.optionsFormArray.controls[event.currentIndex] = previousIndex;

    this.optionsFormArray.updateValueAndValidity();
  }

  public addOption(): void {
    this.optionsFormArray.push(
      new FormControl(0, {
        validators: [Validators.required],
        nonNullable: true,
      }),
    );

    this.optionsFormArray.updateValueAndValidity();
  }

  public removeOption(index: number): void {
    this.optionsFormArray.removeAt(index);

    this.optionsFormArray.updateValueAndValidity();
  }

  private onTouched?() {
    return;
  }

  private onChange?(_: unknown) {
    return;
  }
}
