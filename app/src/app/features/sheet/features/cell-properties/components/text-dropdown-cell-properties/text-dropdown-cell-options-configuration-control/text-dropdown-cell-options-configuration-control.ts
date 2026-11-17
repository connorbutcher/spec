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
import { InputText } from 'primeng/inputtext';
import { takeUntil } from 'rxjs';
import { Destroyable } from '../../../../../../common/destructable/destructable';

@Component({
  selector: 'app-text-dropdown-cell-options-configuration-control',
  standalone: true,
  imports: [ReactiveFormsModule, InputText, ButtonDirective, DragDropModule],
  templateUrl: './text-dropdown-cell-options-configuration-control.html',
  styleUrl: './text-dropdown-cell-options-configuration-control.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(
        () => TextDropdownCellOptionsConfigurationControl,
      ),
      multi: true,
    },
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(
        () => TextDropdownCellOptionsConfigurationControl,
      ),
      multi: true,
    },
  ],
})
export class TextDropdownCellOptionsConfigurationControl
  extends Destroyable
  implements ControlValueAccessor, Validator
{
  @Input()
  public get value(): string[] {
    return this.optionsFormArray.value;
  }
  public set value(values: string[]) {
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
  public readonly optionsFormArray = new FormArray<FormControl<string>>([]);

  writeValue(valueToWrite: string[]): void {
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

  public drop(event: CdkDragDrop<FormArray<FormControl<string>>>) {
    const previousIndex = this.optionsFormArray.controls[event.previousIndex];
    this.optionsFormArray.controls[event.previousIndex] =
      this.optionsFormArray.controls[event.currentIndex];
    this.optionsFormArray.controls[event.currentIndex] = previousIndex;

    this.optionsFormArray.updateValueAndValidity();
  }

  public addOption(): void {
    this.optionsFormArray.push(
      new FormControl('', {
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
