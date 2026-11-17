import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { Component, computed, inject, model, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ButtonDirective } from 'primeng/button';
import { Select } from 'primeng/select';
import { SpecSheetPhaseHelpers } from '../../../../core/helpers/spec-sheet-phase-helpers';
import { User } from '../../../../core/models/domain/access/user';
import { WorkflowState } from '../../../../core/models/domain/enums/workflow-state';
import { WorkflowStep } from '../../../../core/models/domain/workflow/workflow-step';
import { ReassignWorkflowStepActorsRequest } from '../../../../core/models/requests/workflow/reassign-workflow-step-actors-request';
import { PhasesService } from '../../../../core/services/phases.service';
import { UserService } from '../../../../core/services/user.service';
import { DialogHeader } from '../../../common/dialog-header/dialog-header';
import { ElementLabel } from '../../../common/element-label/element-label';
import { ActiveSheetService } from '../../services/active-sheet.service';
import { SheetDataService } from '../../services/sheet-data.service';

@Component({
  selector: 'app-reassign-approvers-dialog',
  imports: [FormsModule, DialogHeader, ButtonDirective, ElementLabel, Select],
  templateUrl: './reassign-approvers-dialog.html',
  styleUrl: './reassign-approvers-dialog.scss',
  host: { class: 'dialog card' },
})
export class ReassignApproversDialog {
  public readonly approvers = model<User[]>([]);
  public readonly approverOptions = computed(() => {
    const phase = this._phasesService.selectedPhase();
    const phases = this._phasesService.specSheetPhases();
    const currentUser = this._userService.user();
    if (!phase || !phases || !currentUser) {
      return [];
    }

    const fullPhase = SpecSheetPhaseHelpers.getPhaseByIdFromArray(
      phases,
      phase.id,
    );
    if (fullPhase?.parentSpecSheetPhaseId) {
      const parentPhase = SpecSheetPhaseHelpers.getPhaseByIdFromArray(
        phases,
        fullPhase.parentSpecSheetPhaseId,
      );
      if (!parentPhase) {
        return [];
      }

      return parentPhase.phaseObservers
        .filter((po) => po.isApprover && po.user.id !== currentUser.id)
        .map((po) => po.user);
    } else {
      return (
        fullPhase?.phaseObservers
          .filter((po) => po.isApprover && po.user.id !== currentUser.id)
          .map((po) => po.user) ?? []
      );
    }
  });
  public readonly hasChanges = computed(() => {
    const originalApprovers = this._dialogData.actors.filter(
      (a) => a.state === WorkflowState.Pending,
    );
    const approvers = this.approvers();

    return (
      approvers.length !== originalApprovers.length ||
      approvers.some(
        (a) => !originalApprovers.some((oa) => oa.actor!.id === a.id),
      )
    );
  });
  public readonly isValid = computed(() => this.approvers().length > 0);
  public readonly isSaving = signal(false);

  private readonly _dialogRef = inject(
    DialogRef<void, ReassignApproversDialog>,
  );
  private readonly _dialogData = inject<WorkflowStep>(DIALOG_DATA);
  private readonly _activeSheetService = inject(ActiveSheetService);
  private readonly _sheetDataService = inject(SheetDataService);
  private readonly _phasesService = inject(PhasesService);
  private readonly _userService = inject(UserService);
  private readonly _messageService = inject(MessageService);

  constructor() {
    const remainingApprovers = this._dialogData.actors.filter(
      (a) => a.state === WorkflowState.Pending,
    );
    this.approvers.set(remainingApprovers.map((a) => a.actor!));
  }

  public async saveChanges(): Promise<void> {
    if (!this.hasChanges() || !this.isValid() || this.isSaving()) {
      return;
    }

    const approvers = this.approvers();
    if (!approvers || approvers.length === 0) {
      return;
    }

    const originalApprovers = this._dialogData.actors.filter(
      (a) => a.state === WorkflowState.Pending,
    );
    const approversToAdd = approvers.filter(
      (a) => !originalApprovers.some((oa) => oa.actor!.id === a.id),
    );
    const approversToRemove = originalApprovers.filter(
      (oa) => !approvers.some((a) => a.id === oa.actor!.id),
    );

    try {
      const request = {
        specSheetId: this._activeSheetService.specSheet().specSheet.id,
        workflowStepId: this._dialogData.id,
        createWorkflowStepActorRequests: approversToAdd.map((a) => a.id),
        deleteWorkflowStepActorRequests: approversToRemove.map((oa) => oa.id),
      } as ReassignWorkflowStepActorsRequest;

      await this._sheetDataService.reassignWorkflowStepActorsAsync(request);

      this._messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: 'Approvers reassigned successfully.',
      });
      this._activeSheetService.triggerReload();

      this.close();
    } catch (error) {
      this._messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'An error occurred while reassigning approvers.',
      });
      return;
    } finally {
      this.isSaving.set(false);
    }
  }

  public close(): void {
    this._dialogRef.close();
  }
}
