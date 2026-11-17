import {
  Component,
  computed,
  effect,
  inject,
  input,
  numberAttribute,
  signal,
  Signal,
} from '@angular/core';
import {
  takeUntilDestroyed,
  toObservable,
  toSignal,
} from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  FormsModule,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MessageService, TreeNode } from 'primeng/api';
import { ButtonDirective } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { SelectButton } from 'primeng/selectbutton';
import { TreeSelect } from 'primeng/treeselect';
import { combineLatest, map } from 'rxjs';
import { SpecSheetPhaseObserver } from '../../../../../../core/models/domain/access/spec-sheet-phase-observer';
import { User } from '../../../../../../core/models/domain/access/user';
import { SpecSheetPhase } from '../../../../../../core/models/domain/types/spec-sheet-phase';
import { SpecSheetType } from '../../../../../../core/models/domain/types/spec-sheet-type';
import { CreateSpecSheetPhaseObserverRequest } from '../../../../../../core/models/requests/access/create-spec-sheet-phase-observer-request';
import { CreateSpecSheetPhaseRequest } from '../../../../../../core/models/requests/types/phase/create-spec-sheet-phase-request';
import { PhasesService } from '../../../../../../core/services/phases.service';
import { SpinnerDialogService } from '../../../../../../core/services/spinner-dialog.service';
import { TypesService } from '../../../../../../core/services/types.service';
import { ElementLabel } from '../../../../../common/element-label/element-label';
import { AdminService } from '../../../../admin.service';
import { PhaseDataService } from '../../phase-data.service';
import { PhasePublishing } from '../phase-publishing/phase-publishing';

interface CreateSpecSheetPhaseForm {
  phase: FormControl<string>;
  phaseShort: FormControl<string>;
  types: FormControl<SpecSheetType[]>;
  parentPhase: FormControl<TreeNode<SpecSheetPhase> | undefined>;
  publishingOption: FormControl<'copy' | 'new'>;
  copyFromPhase: FormControl<SpecSheetPhase | undefined>;
  observers: FormControl<SpecSheetPhaseObserver[]>;
}

@Component({
  selector: 'app-create-phase',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    ButtonDirective,
    InputText,
    ElementLabel,
    TreeSelect,
    SelectButton,
    Select,
    PhasePublishing,
  ],
  templateUrl: './create-phase.html',
  styleUrl: './create-phase.scss',
})
export class CreatePhase {
  public readonly parentId = input.required({ transform: numberAttribute });

  public readonly form: FormGroup<CreateSpecSheetPhaseForm>;
  public readonly hasParent: Signal<boolean>;
  public readonly publishingOption: Signal<'copy' | 'new'>;
  public readonly isCreating = signal(false);
  public readonly phaseOptions = computed<TreeNode[]>(() => {
    const phases = this._phasesService.specSheetPhases();

    const nodes: TreeNode[] = [];
    for (const phase of phases) {
      nodes.push({
        key: `${phase.id}`,
        label: phase.phase,
        data: phase,
        children: phase.phases.map((subPhase) => ({
          key: `${subPhase.id}`,
          label: subPhase.phase,
          data: subPhase,
        })),
      });
    }

    return nodes;
  });
  public readonly publishingOptions = [
    { label: 'Copy From another Phase', value: 'copy' },
    { label: 'New List', value: 'new' },
  ];
  public readonly copyFromPhaseOptions = computed(() =>
    this._phasesService.specSheetPhases().filter((p) => !p.isObsolete),
  );

  private readonly _phasesService = inject(PhasesService);
  private readonly _typesService = inject(TypesService);
  private readonly _phaseDataService = inject(PhaseDataService);
  private readonly _adminService = inject(AdminService);
  private readonly _spinnerDialogService = inject(SpinnerDialogService);
  private readonly _formBuilder = inject(NonNullableFormBuilder);
  private readonly _messageService = inject(MessageService);
  private readonly _router = inject(Router);
  private readonly _activatedRoute = inject(ActivatedRoute);

  constructor() {
    this.form = this.buildForm();

    this.hasParent = toSignal(
      combineLatest([
        toObservable(this.parentId).pipe(map((id) => !!id)),
        this.form.valueChanges.pipe(
          takeUntilDestroyed(),
          map((value) => !!value.parentPhase),
        ),
      ]).pipe(
        map(
          ([isParentIdPresent, hasParentPhase]) =>
            isParentIdPresent || hasParentPhase,
        ),
      ),
      { initialValue: false },
    );

    this.publishingOption = toSignal(
      this.form.controls.publishingOption.valueChanges,
      { initialValue: this.form.controls.publishingOption.value },
    );

    effect(() => {
      const phaseOptions = this.phaseOptions();
      const parentPhaseId = this.parentId();
      if (!parentPhaseId) {
        this.form.patchValue({ parentPhase: undefined });
        return;
      }

      for (const node of phaseOptions) {
        if (node.key === `${parentPhaseId}`) {
          this.form.patchValue({ parentPhase: node });
          break;
        }

        for (const childNode of node.children || []) {
          if (childNode.key === `${parentPhaseId}`) {
            this.form.patchValue({ parentPhase: childNode });
            break;
          }
        }
      }
    });
  }

  public async createAsync(): Promise<void> {
    if (this.form.invalid || this.isCreating()) {
      this._messageService.add({
        severity: 'error',
        summary: 'Invalid Input',
        detail: 'Please correct the errors in the form before submitting.',
      });
      this.form.markAllAsTouched();
      return;
    }

    this.isCreating.set(true);

    const spinnerRef = this._spinnerDialogService.open(
      'Creating Phase',
      'This should only take a moment.',
    );

    try {
      let displayOrder = 0;
      if (this.form.value.parentPhase) {
        displayOrder = this.form.value.parentPhase.data!.phases.length + 1;
      } else {
        const phases = this._phasesService.specSheetPhases();
        displayOrder = phases.length + 1;
      }

      const createSpecSheetPhaseObserverRequests: CreateSpecSheetPhaseObserverRequest[] =
        [];

      if (!this.form.value.parentPhase) {
        if (
          this.form.value.publishingOption === 'copy' &&
          this.form.value.copyFromPhase
        ) {
          const observersToCopy =
            this.form.value.copyFromPhase.phaseObservers.map(
              (observer) =>
                ({
                  specSheetPhaseId: 0,
                  userId: observer.userId,
                  isApprover: observer.isApprover,
                }) as CreateSpecSheetPhaseObserverRequest,
            );
          createSpecSheetPhaseObserverRequests.push(...observersToCopy);
        } else {
          const observersToCreate = this.form.value.observers!.map(
            (observer) => ({
              specSheetPhaseId: 0,
              userId: observer.userId,
              isApprover: observer.isApprover,
            }),
          );
          createSpecSheetPhaseObserverRequests.push(...observersToCreate);
        }
      }

      const request = {
        parentSpecSheetPhaseId: this.form.value.parentPhase?.data?.id ?? null,
        phase: this.form.value.phase,
        phaseShort: this.form.value.phaseShort,
        specSheetTypeIds: this.form.value.types!.map((type) => type.id),
        displayOrder: displayOrder,
        createSpecSheetPhaseObserverRequests,
      } as CreateSpecSheetPhaseRequest;
      await this._phaseDataService.createSpecSheetPhaseAsync(request);

      this._messageService.add({
        severity: 'success',
        summary: 'Phase Created',
        detail: `The spec sheet phase "${request.phase}" has been created successfully.`,
      });

      this._phasesService.refresh();
      this.close();
    } catch (error) {
      this._messageService.add({
        severity: 'error',
        summary: 'Creation Failed',
        detail: 'An error occurred while creating the spec sheet phase.',
      });
    } finally {
      this.isCreating.set(false);
      spinnerRef.close();
    }
  }

  public close(): void {
    this._router.navigate(['..'], { relativeTo: this._activatedRoute });
  }

  public hasError(
    control: keyof CreateSpecSheetPhaseForm,
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

  public get users(): Signal<User[]> {
    return this._adminService.users;
  }

  private buildForm(): FormGroup<CreateSpecSheetPhaseForm> {
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
        parentPhase: this._formBuilder.control<
          TreeNode<SpecSheetPhase> | undefined
        >(undefined),
        publishingOption: this._formBuilder.control<'copy' | 'new'>('copy', {
          validators: [Validators.required],
        }),
        copyFromPhase: this._formBuilder.control<SpecSheetPhase | undefined>(
          undefined,
        ),
        observers: this._formBuilder.control<SpecSheetPhaseObserver[]>([]),
      },
      { validators: this.uniquePhaseValidator() },
    );
  }

  private uniquePhaseValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const form = control as FormGroup<CreateSpecSheetPhaseForm>;
      const phaseControl = form.get('phase');
      const parentPhaseControl = form.get('parentPhase');

      if (!phaseControl || !parentPhaseControl) {
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

      const duplicate = siblings.some(
        (phase) => phase.phase.trim().toLowerCase() === normalizedPhase,
      );

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
}
