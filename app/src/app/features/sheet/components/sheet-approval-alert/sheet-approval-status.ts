import { Dialog } from '@angular/cdk/dialog';
import { Component, computed, inject, Injector } from '@angular/core';
import { DateTime } from 'luxon';
import { MessageService } from 'primeng/api';
import { ButtonDirective } from 'primeng/button';
import { Tooltip } from 'primeng/tooltip';
import { firstValueFrom } from 'rxjs';
import { DecideSubmissionRequest } from '../../../../core/models/requests/sheets/decide-submission-request';
import { ApproverDecision } from '../../../../core/types/sheets/approver-decision.enum';
import { SubmissionStatus } from '../../../../core/types/sheets/submission-status.enum';
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
    const steps = specSheet.submissions;

    if (this.isApprovalRequired()) {
      return { label: 'Approval required', severity: 'warn' };
    }

    if (steps.some((step) => step.status === SubmissionStatus.Pending)) {
      return { label: 'Approval pending', severity: 'warn' };
    }

    const latestStep = [...steps].sort(
      (left, right) =>
        DateTime.fromISO(right.submittedDateTime).toMillis() -
        DateTime.fromISO(left.submittedDateTime).toMillis(),
    )[0];

    if (latestStep?.status === SubmissionStatus.Rejected) {
      return { label: 'Approval rejected', severity: 'danger' };
    }

    if (latestStep?.status === SubmissionStatus.Approved) {
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
    const pendingSteps = specSheet.submissions.filter(
      (ws) => ws.status === SubmissionStatus.Pending,
    );
    if (!currentUser || pendingSteps.length === 0) {
      return undefined;
    }

    const approvers = pendingSteps.flatMap((ws) => ws.approvers);
    return approvers.find(
      (approver) =>
        approver.userId === currentUser.id &&
        approver.decision === ApproverDecision.Pending,
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
        sheetId: this._activeSheetService.specSheet().id,
        decision: approve
          ? ApproverDecision.Approved
          : ApproverDecision.Rejected,
        comment: comment ?? undefined,
      } as DecideSubmissionRequest;

      await this._sheetDataService.decideSubmissionAsync(request);

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
