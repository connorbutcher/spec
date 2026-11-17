import { Component, forwardRef, inject, Input, Signal } from '@angular/core';
import {
  AbstractControl,
  ControlValueAccessor,
  FormControl,
  FormGroup,
  FormsModule,
  NG_VALIDATORS,
  NG_VALUE_ACCESSOR,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validator,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { TreeNode } from 'primeng/api';
import { InputText } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { takeUntil } from 'rxjs';
import { SpecSheetPhase } from '../../../../../../core/models/domain/types/spec-sheet-phase';
import { SpecSheetType } from '../../../../../../core/models/domain/types/spec-sheet-type';
import { PhasesService } from '../../../../../../core/services/phases.service';
import { TypesService } from '../../../../../../core/services/types.service';
import { Destroyable } from '../../../../../common/destructable/destructable';
import { ElementLabel } from '../../../../../common/element-label/element-label';

interface UpdateSpecSheetPhaseForm {
  phase: FormControl<string>;
  phaseShort: FormControl<string>;
  types: FormControl<SpecSheetType[]>;
}

@Component({
  selector: 'app-update-phase-general-info',
  imports: [FormsModule, ReactiveFormsModule, InputText, Select, ElementLabel],
  templateUrl: './update-phase-general-info.html',
  styleUrl: './update-phase-general-info.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => UpdatePhaseGeneralInfo),
      multi: true,
    },
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => UpdatePhaseGeneralInfo),
      multi: true,
    },
  ],
})
export class UpdatePhaseGeneralInfo
  extends Destroyable
  implements ControlValueAccessor, Validator
{
  @Input()
  public get value(): SpecSheetPhase {
    return {
      id: this._value!.id,
      phase: this.form.controls.phase.value ?? '',
      phaseShort: this.form.controls.phaseShort.value ?? '',
      displayOrder: this._value!.displayOrder,
      isObsolete: this._value!.isObsolete,
      types: this.form.controls.types.value ?? [],
      phases: this._value!.phases,
      phaseObservers: this._value!.phaseObservers ?? [],
    };
  }
  public set value(value: SpecSheetPhase) {
    this._value = value;

    this.form.patchValue({
      phase: value.phase,
      phaseShort: value.phaseShort,
      types: this._typesService
        .specSheetTypes()
        .filter((type) =>
          value.types.some((phaseType) => phaseType.id === type.id),
        ),
    });
    this.form.updateValueAndValidity();
  }

  public readonly form: FormGroup<UpdateSpecSheetPhaseForm>;

  private readonly _formBuilder = inject(NonNullableFormBuilder);
  private readonly _phasesService = inject(PhasesService);
  private readonly _typesService = inject(TypesService);

  private _value?: SpecSheetPhase;

  constructor() {
    super();

    this.form = this.buildForm();
  }

  writeValue(valueToWrite: SpecSheetPhase): void {
    this.value = valueToWrite;
  }

  registerOnChange(fn: any): void {
    this.form.valueChanges
      .pipe(takeUntil(this.destroy))
      .subscribe(() => fn(this.value));
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  validate(control: AbstractControl): ValidationErrors | null {
    return this.form.valid ? null : { invalid: true };
  }

  public hasError(
    control: keyof UpdateSpecSheetPhaseForm,
    error: string,
  ): boolean {
    const formControl = this.form.get(control);
    return formControl
      ? formControl.hasError(error) && formControl.touched
      : false;
  }

  public formHasError(error: string): boolean {
    return this.form.hasError(error) && this.form.touched;
  }

  public get specSheetTypes(): Signal<SpecSheetType[]> {
    return this._typesService.specSheetTypes;
  }

  private buildForm(): FormGroup<UpdateSpecSheetPhaseForm> {
    return this._formBuilder.group(
      {
        phase: this._formBuilder.control('', [
          Validators.required,
          Validators.minLength(1),
          Validators.maxLength(64),
        ]),
        phaseShort: this._formBuilder.control('', [
          Validators.required,
          Validators.minLength(1),
          Validators.maxLength(8),
        ]),
        types: this._formBuilder.control<SpecSheetType[]>(
          [],
          [Validators.required, Validators.minLength(1)],
        ),
      },
      { validators: this.uniquePhaseValidator() },
    );
  }

  private uniquePhaseValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const form = control as FormGroup<UpdateSpecSheetPhaseForm>;
      const phaseControl = form.get('phase');
      const parentPhaseControl = form.get('parentPhase');

      if (!phaseControl || !parentPhaseControl || !this._value) {
        return null;
      }

      const rawPhase = (phaseControl.value ?? '').trim();
      if (!rawPhase) {
        return null;
      }

      const normalizedPhase = rawPhase.toLowerCase();
      const parentNode = parentPhaseControl.value as
        | TreeNode<SpecSheetPhase>
        | undefined;

      const phases = this._phasesService.specSheetPhases();
      const parentPhase = parentNode?.data?.id
        ? this.findPhaseById(phases, parentNode.data.id)
        : undefined;

      const siblings = parentPhase ? parentPhase.phases : phases;
      const duplicate = siblings
        .filter((phase) => phase.id !== this._value!.id)
        .some((phase) => phase.phase.trim().toLowerCase() === normalizedPhase);

      return duplicate ? { duplicatePhase: true } : null;
    };
  }

  private findPhaseById(
    phases: SpecSheetPhase[],
    id: number,
  ): SpecSheetPhase | undefined {
    for (const phase of phases) {
      if (phase.id === id) {
        return phase;
      }

      const childPhase = this.findPhaseById(phase.phases, id);
      if (childPhase) {
        return childPhase;
      }
    }

    return undefined;
  }

  private onTouched?() {
    return;
  }

  private onChange?(_: unknown) {
    return;
  }
}
