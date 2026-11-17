import { CdkDragDrop, DragDropModule } from '@angular/cdk/drag-drop';
import {
  Component,
  computed,
  effect,
  forwardRef,
  inject,
  input,
  Input,
  linkedSignal,
  model,
  signal,
  Signal,
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  ControlValueAccessor,
  FormControl,
  FormGroup,
  FormsModule,
  NG_VALIDATORS,
  NG_VALUE_ACCESSOR,
  NonNullableFormBuilder,
  ValidationErrors,
  Validator,
  Validators,
} from '@angular/forms';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { TabsModule } from 'primeng/tabs';
import { combineLatest, takeUntil } from 'rxjs';
import { SpecSheetPhaseObserver } from '../../../../../../core/models/domain/access/spec-sheet-phase-observer';
import { User } from '../../../../../../core/models/domain/access/user';
import { SpecSheetType } from '../../../../../../core/models/domain/types/spec-sheet-type';
import { TypesService } from '../../../../../../core/services/types.service';
import { Destroyable } from '../../../../../common/destructable/destructable';
import { InfoBlock } from '../../../../../common/info-block/info-block';
import { AdminService } from '../../../../admin.service';

interface PhasePublishingForm {
  observers: FormControl<SpecSheetPhaseObserver[]>;
  approvers: FormControl<SpecSheetPhaseObserver[]>;
}

@Component({
  selector: 'app-phase-publishing',
  imports: [
    FormsModule,
    TabsModule,
    InputText,
    DragDropModule,
    Message,
    InfoBlock,
  ],
  templateUrl: './phase-publishing.html',
  styleUrl: './phase-publishing.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => PhasePublishing),
      multi: true,
    },
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => PhasePublishing),
      multi: true,
    },
  ],
})
export class PhasePublishing
  extends Destroyable
  implements ControlValueAccessor, Validator
{
  @Input()
  public get value(): SpecSheetPhaseObserver[] {
    return [
      ...(this.form.controls.observers.value ?? []),
      ...(this.form.controls.approvers.value ?? []),
    ];
  }
  public set value(value: SpecSheetPhaseObserver[]) {
    this._value = value;

    const approvers = value.filter((observer) => observer.isApprover);
    const observers = value.filter((observer) => !observer.isApprover);

    this.form.patchValue({
      observers,
      approvers,
    });
    this.form.updateValueAndValidity();

    this.observers.set(observers);
    this.approvers.set(approvers);
  }
  public readonly isRequired = input.required<boolean>();
  public readonly filter = model<string>('');

  public readonly form: FormGroup<PhasePublishingForm>;
  public readonly availableUsers = linkedSignal<User[]>(() =>
    this._adminService.users(),
  );
  public readonly usersInUse = computed(() => [
    ...this.observers(),
    ...this.approvers(),
  ]);
  public readonly observers = signal<SpecSheetPhaseObserver[]>([]);
  public readonly approvers = signal<SpecSheetPhaseObserver[]>([]);

  private readonly _formBuilder = inject(NonNullableFormBuilder);
  private readonly _typesService = inject(TypesService);
  private readonly _adminService = inject(AdminService);

  private _value?: SpecSheetPhaseObserver[];

  constructor() {
    super();

    this.form = this.buildForm();

    effect(() => {
      const isRequired = this.isRequired();
      if (isRequired) {
        this.form.controls.observers.setValidators(Validators.required);
        this.form.controls.approvers.setValidators(Validators.required);
      } else {
        this.form.controls.observers.clearValidators();
        this.form.controls.approvers.clearValidators();
      }

      this.form.updateValueAndValidity();
    });

    combineLatest([
      toObservable(this._adminService.users),
      this.form.valueChanges.pipe(takeUntilDestroyed()),
      toObservable(this.filter),
    ]).subscribe(([users, value, filter]) => {
      const selectedUserIds = [
        ...(value.observers?.map((observer) => observer.userId) ?? []),
        ...(value.approvers?.map((observer) => observer.userId) ?? []),
      ];
      this.availableUsers.set(
        users.filter(
          (user) =>
            !selectedUserIds.includes(user.id) &&
            user.displayName.toLowerCase().includes(filter.toLowerCase()),
        ),
      );

      this.observers.set(value.observers ?? []);
      this.approvers.set(value.approvers ?? []);
    });
  }

  public onUserDropped(
    event: CdkDragDrop<any, any, SpecSheetPhaseObserver>,
  ): void {
    if (event.previousContainer === event.container) {
      return;
    }

    const item = event.item.data;
    if (item.isApprover) {
      this.form.controls.approvers.patchValue(
        this.form.controls.approvers.value?.filter(
          (approver) => approver.userId !== item.userId,
        ) ?? [],
      );
    } else {
      this.form.controls.observers.patchValue(
        this.form.controls.observers.value?.filter(
          (observer) => observer.userId !== item.userId,
        ) ?? [],
      );
    }
  }

  public onObserverDropped(
    event: CdkDragDrop<any, any, User | SpecSheetPhaseObserver>,
    type: 'observer' | 'approver',
  ): void {
    if (event.previousContainer === event.container) {
      return;
    }

    if ('isApprover' in event.item.data) {
      const observer = event.item.data as SpecSheetPhaseObserver;
      if (observer.isApprover) {
        this.form.controls.approvers.patchValue(
          this.form.controls.approvers.value?.filter(
            (approver) => approver.userId !== observer.userId,
          ) ?? [],
        );
        this.form.controls.observers.patchValue([
          ...(this.form.controls.observers.value ?? []),
          { ...observer, isApprover: false },
        ]);
      } else {
        this.form.controls.observers.patchValue(
          this.form.controls.observers.value?.filter(
            (obs) => obs.userId !== observer.userId,
          ) ?? [],
        );
        this.form.controls.approvers.patchValue([
          ...(this.form.controls.approvers.value ?? []),
          { ...observer, isApprover: true },
        ]);
      }
    } else {
      const observer = {
        id: 0,
        specSheetPhaseId: 0,
        isApprover: type === 'approver',
        userId: event.item.data.id,
        user: event.item.data,
      } as SpecSheetPhaseObserver;

      if (type === 'observer') {
        this.form.controls.observers.patchValue([
          ...this.form.controls.observers.value!,
          observer,
        ]);
      } else {
        this.form.controls.approvers.patchValue([
          ...this.form.controls.approvers.value!,
          observer,
        ]);
      }
    }
  }

  writeValue(valueToWrite: SpecSheetPhaseObserver[]): void {
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

  public hasError(control: keyof PhasePublishingForm, error: string): boolean {
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

  private buildForm(): FormGroup<PhasePublishingForm> {
    return this._formBuilder.group({
      observers: this._formBuilder.control<SpecSheetPhaseObserver[]>(
        [],
        [Validators.required],
      ),
      approvers: this._formBuilder.control<SpecSheetPhaseObserver[]>(
        [],
        [Validators.required],
      ),
    });
  }

  private onTouched?() {
    return;
  }

  private onChange?(_: unknown) {
    return;
  }
}
