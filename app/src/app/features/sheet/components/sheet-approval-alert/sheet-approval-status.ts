import { Dialog } from '@angular/cdk/dialog';
import { Component, computed, inject, Injector } from '@angular/core';
import { DateTime } from 'luxon';
import { MessageService } from 'primeng/api';
import { ButtonDirective } from 'primeng/button';
import { Tooltip } from 'primeng/tooltip';
import { firstValueFrom } from 'rxjs';
import { WorkflowState } from '../../../../core/models/domain/enums/workflow-state';
import { UpdateWorkflowStepActorRequest } from '../../../../core/models/requests/workflow/update-workflow-step-actor-request';
import { SpinnerDialogService } from '../../../../core/services/spinner-dialog.service';
import { UserService } from '../../../../core/services/user.service';
import { SidePanelService } from '../../../side-panel/side-panel.service';
import { RejectSpecSheetDialog } from '../../dialogs/reject-spec-sheet-dialog/reject-spec-sheet-dialog';
import { ApprovalSidePanel } from '../../features/approval/approval-side-panel/approval-side-panel';
import { ActiveSheetService } from '../../services/active-sheet.service';
import { SheetDataService } from '../../services/sheet-data.service';
@Component({
  selector: 'app-sheet-approval-status',
  imports: [ButtonDirective, Tooltip],
  templateUrl: './sheet-approval-status.html',
  styleUrl: './sheet-approval-status.scss',
})
export class SheetApprovalStatus {
  public readonly approvalStatus = computed<{
    label: string;
    severity: 'secondary' | 'warn' | 'success' | 'danger';
  }>(() => {
    const specSheet = this._activeSheetService.specSheet();
    const steps = specSheet.specSheet.workflowSteps;

    if (this.isApprovalRequired()) {
      return { label: 'Approval required', severity: 'warn' };
    }

    if (steps.some((step) => step.state === WorkflowState.Pending)) {
      return { label: 'Approval pending', severity: 'warn' };
    }

    const latestStep = [...steps].sort(
      (left, right) =>
        DateTime.fromISO(right.startedDateTime).toMillis() -
        DateTime.fromISO(left.startedDateTime).toMillis(),
    )[0];

    if (latestStep?.state === WorkflowState.Rejected) {
      return { label: 'Approval rejected', severity: 'danger' };
    }

    if (latestStep?.state === WorkflowState.Approved) {
      return { label: 'Approved', severity: 'success' };
    }

    return {
      label: specSheet.requiresApprovalWorkflow
        ? 'Approval not started'
        : 'Approval not required',
      severity: 'secondary',
    };
  });

  public readonly isApprovalRequired = computed(() => {
    const specSheet = this._activeSheetService.specSheet();
    const approval = this._approval();
    if (
      !approval ||
      !specSheet ||
      (!specSheet.requiresApprovalWorkflow &&
        !specSheet.hasApprovalWorkflowStarted &&
        specSheet.isPublished()) ||
      specSheet.hasChanges() ||
      !specSheet.isValid()
    ) {
      return false;
    }

    return true;
  });
  private readonly _approval = computed(() => {
    const specSheet = this._activeSheetService.specSheet();
    const currentUser = this._userService.user();
    const workflowSteps = specSheet.specSheet.workflowSteps;
    const pendingSteps = workflowSteps.filter(
      (ws) => ws.state === WorkflowState.Pending,
    );
    if (!currentUser || pendingSteps.length === 0) {
      return undefined;
    }

    const actors = pendingSteps.flatMap((ws) => ws.actors);
    return actors.find(
      (actor) =>
        actor.actor?.id === currentUser.id &&
        actor.state === WorkflowState.Pending,
    );
  });

  private readonly _activeSheetService = inject(ActiveSheetService);
  private readonly _sheetDataService = inject(SheetDataService);
  private readonly _userService = inject(UserService);
  private readonly _spinnerService = inject(SpinnerDialogService);
  private readonly _dialog = inject(Dialog);
  private readonly _injector = inject(Injector);
  private readonly _messageService = inject(MessageService);
  private readonly _sidePanelService = inject(SidePanelService);

  public viewApprovalStatus(): void {
    if (this._sidePanelService.activePanel()?.component === ApprovalSidePanel) {
      this._sidePanelService.setShowPanel(true);
      return;
    }

    this._sidePanelService.push(
      ApprovalSidePanel,
      {},
      'spec-sheet-approval-status',
    );
  }

  public async updateStateAsync(approve: boolean): Promise<void> {
    const approval = this._approval();
    if (!this.isApprovalRequired() || !approval) {
      return;
    }

    let comment = null;
    if (!approve) {
      // need to get their feedback as for why they rejected it.
      const rejectReasonDialogRef = this._dialog.open<
        string | undefined,
        unknown,
        RejectSpecSheetDialog
      >(RejectSpecSheetDialog, { injector: this._injector, minWidth: '50%' });
      const rejectReason = await firstValueFrom(rejectReasonDialogRef.closed);
      if (!rejectReason) {
        return;
      }

      comment = rejectReason;
    }

    const ref = this._spinnerService.open(
      'Updating approval state',
      'This should not take long',
    );

    try {
      const request = {
        specSheetId: this._activeSheetService.specSheet().specSheet.id,
        workflowStepId: approval.workflowStepId,
        workflowStepActorId: approval.id,
        state: approve ? WorkflowState.Approved : WorkflowState.Rejected,
        comment,
      } as UpdateWorkflowStepActorRequest;

      await this._sheetDataService.updateApprovalStateAsync(request);

      this._activeSheetService.triggerReload();
      this._messageService.add({
        severity: 'success',
        summary: `Successfully ${approve ? 'approved' : 'rejected'} the spec sheet`,
      });
    } catch (err) {
      this._messageService.add({
        severity: 'error',
        summary: `Error ${approve ? 'approving' : 'rejecting'} the spec sheet`,
        detail: (err as Error).message,
      });
    } finally {
      ref.close();
    }
  }
}
