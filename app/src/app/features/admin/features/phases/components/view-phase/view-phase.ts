import { Dialog } from '@angular/cdk/dialog';
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
  FormControl,
  FormGroup,
  FormsModule,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ButtonDirective } from 'primeng/button';
import { TabsModule } from 'primeng/tabs';
import { combineLatest, filter, firstValueFrom, map } from 'rxjs';
import { SpecSheetPhaseHelpers } from '../../../../../../core/helpers/spec-sheet-phase-helpers';
import { SpecSheetPhaseObserver } from '../../../../../../core/models/domain/access/spec-sheet-phase-observer';
import { SpecSheetPhase } from '../../../../../../core/models/domain/types/spec-sheet-phase';
import { CreateSpecSheetPhaseObserverRequest } from '../../../../../../core/models/requests/access/create-spec-sheet-phase-observer-request';
import { UpdateSpecSheetPhaseObserverRequest } from '../../../../../../core/models/requests/access/update-spec-sheet-phase-observer-request';
import { UpdateSpecSheetPhaseRequest } from '../../../../../../core/models/requests/types/phase/update-spec-sheet-phase-request';
import { PhasesService } from '../../../../../../core/services/phases.service';
import { SpinnerDialogService } from '../../../../../../core/services/spinner-dialog.service';
import { ConfirmationDialog } from '../../../../../common/confirmation-dialog/confirmation-dialog';
import { ConfirmationDialogData } from '../../../../../common/confirmation-dialog/models/confirmation-dialog-data.interface';
import { PhaseDataService } from '../../phase-data.service';
import { PhaseService } from '../../phase.service';
import { UpdatePhaseGeneralInfo } from '../phase-general-info/update-phase-general-info';
import { PhasePublishing } from '../phase-publishing/phase-publishing';

interface PhaseForm {
  phase: FormControl<SpecSheetPhase>;
  publishing: FormControl<SpecSheetPhaseObserver[]>;
}

@Component({
  selector: 'app-view-phase',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    TabsModule,
    ButtonDirective,
    PhasePublishing,
    UpdatePhaseGeneralInfo,
  ],
  templateUrl: './view-phase.html',
  styleUrl: './view-phase.scss',
})
export class ViewPhase {
  public readonly id = input.required({ transform: numberAttribute });
  public readonly phase = computed(() =>
    SpecSheetPhaseHelpers.getPhaseByIdFromArray(
      this._phasesService.specSheetPhases(),
      this.id(),
    ),
  );
  public readonly form: FormGroup<PhaseForm>;
  public readonly isSaving = signal(false);
  public readonly phaseHasChanges: Signal<boolean>;

  private readonly _phasesService = inject(PhasesService);
  private readonly _formBuilder = inject(NonNullableFormBuilder);
  private readonly _messageService = inject(MessageService);
  private readonly _phaseDataService = inject(PhaseDataService);
  private readonly _phaseService = inject(PhaseService);
  private readonly _spinnerDialogService = inject(SpinnerDialogService);
  private readonly _dialog = inject(Dialog);

  constructor() {
    this.form = this.buildForm();

    effect(() => {
      const phase = this.phase();
      if (!phase) {
        return;
      }

      this._phaseService.activePhase.set(phase);
      this.form.patchValue({
        phase,
        publishing: phase.phaseObservers,
      });

      if (phase.parentSpecSheetPhaseId) {
        this.form.controls.publishing.removeValidators(Validators.required);
      } else {
        this.form.controls.publishing.setValidators(Validators.required);
      }

      this.form.updateValueAndValidity();
    });

    const formValueChanges$ = this.form.valueChanges.pipe(
      takeUntilDestroyed(),
      filter(() => this.form.valid),
    );
    const phase$ = toObservable(this.phase);
    const id$ = toObservable(this.id);

    this.phaseHasChanges = toSignal(
      combineLatest([formValueChanges$, phase$, id$]).pipe(
        map(([formValue, phase]) => {
          if (!formValue || !phase) {
            return false;
          }

          return (
            this.phaseHasChangesFromFormValue(formValue.phase!, phase) ||
            this.publishingHasChangesFromFormValue(
              formValue.publishing!,
              phase.phaseObservers,
            )
          );
        }),
      ),
      { initialValue: false },
    );
  }

  public async updateAsync(): Promise<void> {
    const phase = this.phase();
    if (this.form.invalid || this.isSaving() || !phase) {
      this._messageService.add({
        severity: 'error',
        summary: 'Invalid Input',
        detail: 'Please correct the errors in the form before submitting.',
      });
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);

    const spinnerRef = this._spinnerDialogService.open(
      'Updating Phase',
      'This should only take a moment.',
    );

    try {
      const newSpecSheetTypeIds: number[] = [];
      const removedSpecSheetTypeIds: number[] = [];
      const originalTypeIds = phase.types.map((t) => t.id);
      const selectedTypeIds = this.form.value.phase!.types!.map((t) => t.id);
      for (const typeId of selectedTypeIds) {
        if (!originalTypeIds.includes(typeId)) {
          newSpecSheetTypeIds.push(typeId);
        }
      }

      for (const typeId of originalTypeIds) {
        if (!selectedTypeIds.includes(typeId)) {
          removedSpecSheetTypeIds.push(typeId);
        }
      }

      const createSpecSheetPhaseObserverRequests = this.form.value
        .publishing!.filter(
          (o) => !phase.phaseObservers.some((obs) => obs.userId === o.userId),
        )
        .map(
          (o) =>
            ({
              specSheetPhaseId: phase.id,
              userId: o.userId,
              isApprover: o.isApprover,
            }) as CreateSpecSheetPhaseObserverRequest,
        );
      const updateSpecSheetPhaseObserverRequests = this.form.value
        .publishing!.filter((o) =>
          phase.phaseObservers.some(
            (obs) => obs.userId === o.userId && obs.isApprover !== o.isApprover,
          ),
        )
        .map(
          (o) =>
            ({
              id: o.id,
              isApprover: o.isApprover,
            }) as UpdateSpecSheetPhaseObserverRequest,
        );
      const removeSpecSheetPhaseObserverIds = phase.phaseObservers
        .filter(
          (obs) =>
            !this.form.value.publishing!.some((o) => o.userId === obs.userId),
        )
        .map((o) => o.id);

      const request = {
        id: phase.id,
        parentSpecSheetPhaseId: phase.parentSpecSheetPhaseId,
        phase: this.form.value.phase!.phase,
        phaseShort: this.form.value.phase!.phaseShort,
        displayOrder: phase.displayOrder,
        isObsolete: false,
        newSpecSheetTypeIds,
        removedSpecSheetTypeIds,
        createSpecSheetPhaseObserverRequests,
        updateSpecSheetPhaseObserverRequests,
        removeSpecSheetPhaseObserverIds,
      } as UpdateSpecSheetPhaseRequest;
      await this._phaseDataService.updateSpecSheetPhaseAsync(request);

      this._messageService.add({
        severity: 'success',
        summary: 'Update Successful',
        detail: 'The spec sheet phase has been updated successfully.',
      });

      this._phasesService.refresh();
    } catch (error) {
      this._messageService.add({
        severity: 'error',
        summary: 'Update Failed',
        detail: 'An error occurred while updating the spec sheet phase.',
      });
    } finally {
      this.isSaving.set(false);
      spinnerRef.close();
    }
  }

  public async markAsObsoleteAsync(): Promise<void> {
    const phase = this.phase();
    if (this.isSaving() || !phase) {
      this._messageService.add({
        severity: 'error',
        summary: 'Operation In Progress',
        detail: 'Please wait for the current operation to complete.',
      });
      return;
    }

    this.isSaving.set(true);

    const confirmationDialog = this._dialog.open<
      boolean,
      ConfirmationDialogData,
      ConfirmationDialog
    >(ConfirmationDialog, {
      data: {
        title: 'Confirm Mark as Obsolete',
        message:
          'Are you sure you want to mark this phase as obsolete? You will no longer be able to create new spec sheets for this phase. \n This action cannot be undone.',
        confirmText: 'Mark as Obsolete',
        cancelText: 'Cancel',
        confirmSeverity: 'danger',
      },
    });
    const confirmation = await firstValueFrom(confirmationDialog.closed);
    if (!confirmation) {
      this.isSaving.set(false);
      return;
    }

    const spinnerRef = this._spinnerDialogService.open(
      'Marking Phase as Obsolete',
      'This should only take a moment.',
    );

    try {
      const request = {
        id: phase.id,
        parentSpecSheetPhaseId: phase.parentSpecSheetPhaseId,
        phase: phase.phase,
        phaseShort: phase.phaseShort,
        isObsolete: true,
        newSpecSheetTypeIds: [],
        removedSpecSheetTypeIds: [],
        displayOrder: phase.displayOrder,
        createSpecSheetPhaseObserverRequests: [],
        updateSpecSheetPhaseObserverRequests: [],
        removeSpecSheetPhaseObserverIds: [],
      } as UpdateSpecSheetPhaseRequest;
      await this._phaseDataService.updateSpecSheetPhaseAsync(request);

      this._messageService.add({
        severity: 'success',
        summary: 'Obsolete Successful',
        detail: 'The spec sheet phase has been marked as obsolete.',
      });
      this._phasesService.refresh();
    } catch (error) {
      this._messageService.add({
        severity: 'error',
        summary: 'Obsolete Failed',
        detail:
          'An error occurred while marking the spec sheet phase as obsolete.',
      });
    } finally {
      this.isSaving.set(false);
      spinnerRef.close();
    }
  }

  private buildForm(): FormGroup<PhaseForm> {
    return this._formBuilder.group({
      phase: this._formBuilder.control<SpecSheetPhase>(undefined!, [
        Validators.required,
      ]),
      publishing: this._formBuilder.control<SpecSheetPhaseObserver[]>(
        [],
        [Validators.required],
      ),
    });
  }

  private phaseHasChangesFromFormValue(
    formValue: SpecSheetPhase,
    phase: SpecSheetPhase,
  ): boolean {
    return (
      formValue.phase !== phase.phase ||
      formValue.phaseShort !== phase.phaseShort ||
      formValue.isObsolete !== phase.isObsolete ||
      formValue.types.length !== phase.types.length ||
      formValue.types.some((type) => !phase.types.some((t) => t.id === type.id))
    );
  }

  private publishingHasChangesFromFormValue(
    formValue: SpecSheetPhaseObserver[],
    observers: SpecSheetPhaseObserver[],
  ): boolean {
    if (formValue.length !== observers.length) {
      return true;
    }

    const formValueUserIds = formValue.map((o) => o.userId);
    const observerUserIds = observers.map((o) => o.userId);
    const newObserverUserIds = formValueUserIds.filter(
      (id) => !observerUserIds.includes(id),
    );
    const removedObserverUserIds = observerUserIds.filter(
      (id) => !formValueUserIds.includes(id),
    );
    const updatedObservers = formValue.filter((o) =>
      observers.some(
        (obs) => obs.userId === o.userId && obs.isApprover !== o.isApprover,
      ),
    );

    return (
      newObserverUserIds.length > 0 ||
      removedObserverUserIds.length > 0 ||
      updatedObservers.length > 0
    );
  }
}
