import { Component, computed, inject } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { Tooltip } from 'primeng/tooltip';
import { UserService } from '../../../../core/services/user.service';
import { ApproverDecision } from '../../../../core/types/sheets/approver-decision.enum';
import { SubmissionStatus } from '../../../../core/types/sheets/submission-status.enum';
import { SidePanelService } from '../../../side-panel/side-panel.service';
import { ApprovalSidePanel } from '../../features/approval/approval-side-panel/approval-side-panel';
import { ActiveSheetService } from '../../services/active-sheet.service';
import { SheetSubmissionService } from '../../services/sheet-submission.service';

/**
 * How many submissions on the sheet are awaiting approval, with Approve and Reject for the first one the current user
 * still has to decide. Every submission is listed in the approval side panel.
 */
@Component({
  selector: 'app-sheet-approval-status',
  imports: [ButtonDirective, Tooltip],
  templateUrl: './sheet-approval-status.html',
  styleUrl: './sheet-approval-status.scss',
})
export class SheetApprovalStatus {
  public readonly pendingSubmissions = computed(() =>
    this._activeSheetService
      .specSheet()
      .submissions.filter((s) => s.status === SubmissionStatus.Pending),
  );
  public readonly approvalStatus = computed<{
    label: string;
    severity: 'secondary' | 'warn' | 'success' | 'danger';
  }>(() => {
    const pending = this.pendingSubmissions().length;
    if (this.submissionToDecide()) {
      return { label: 'Approval required', severity: 'warn' };
    }

    if (pending > 0) {
      return {
        label: pending === 1 ? '1 awaiting approval' : `${pending} awaiting approval`,
        severity: 'warn',
      };
    }

    return { label: 'Approvals', severity: 'secondary' };
  });
  /** The first pending submission the current user is an approver of and hasn't decided. */
  public readonly submissionToDecide = computed(() => {
    const currentUser = this._userService.user();
    if (!currentUser || this._activeSheetService.hasChanges()) {
      return undefined;
    }

    return this.pendingSubmissions().find((submission) =>
      submission.approvers.some(
        (approver) =>
          approver.userId === currentUser.id &&
          approver.decision === ApproverDecision.Pending,
      ),
    );
  });
  public readonly decideTooltip = computed(() => {
    const submission = this.submissionToDecide();
    return submission ? SheetSubmissionService.whoseChanges(submission) : '';
  });

  private readonly _activeSheetService = inject(ActiveSheetService);
  private readonly _submissionService = inject(SheetSubmissionService);
  private readonly _userService = inject(UserService);
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
    const submission = this.submissionToDecide();
    if (!submission) {
      return;
    }

    await this._submissionService.decideAsync(submission, approve);
  }
}
