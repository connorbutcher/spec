import { Dialog } from '@angular/cdk/dialog';
import { DatePipe } from '@angular/common';
import { Component, computed, inject, Injector, input } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { Tooltip } from 'primeng/tooltip';
import { ApproverDecision } from '../../../../../../core/types/sheets/approver-decision.enum';
import { SubmissionStatus } from '../../../../../../core/types/sheets/submission-status.enum';
import { UserService } from '../../../../../../core/services/user.service';
import { ReassignApproversDialog } from '../../../../dialogs/reassign-approvers-dialog/reassign-approvers-dialog';
import { ActiveSheetService } from '../../../../services/active-sheet.service';
import { SheetSubmissionApproverStatus } from '../sheet-submission-approver-status/sheet-submission-approver-status';
import { ApprovalStatusTag } from '../approval-status-tag/approval-status-tag';
import { SheetSubmission } from '../../../../../../core/models/domain/sheets/sheet-submission';

@Component({
  selector: 'app-sheet-submission-status',
  imports: [
    DatePipe,
    SheetSubmissionApproverStatus,
    ApprovalStatusTag,
    ButtonDirective,
    Tooltip,
  ],
  templateUrl: './sheet-submission-status.html',
  styleUrl: './sheet-submission-status.scss',
})
export class SheetSubmissionStatus {
  public readonly submission = input.required<SheetSubmission>();
  public readonly canToggle = input.required<boolean>();
  public readonly heading = input('Current approval');
  public readonly approvers = computed(() => {
    const approvers = [...this.submission().approvers];
    if (this.submission().status === SubmissionStatus.Pending) {
      approvers.sort(
        (left, right) =>
          Number(right.decision === ApproverDecision.Pending) -
          Number(left.decision === ApproverDecision.Pending),
      );
    }
    return approvers;
  });
  public readonly approvalCounts = computed(() => {
    const approvers = this.submission().approvers;
    return [
      { state: ApproverDecision.Approved, label: 'approved' },
      { state: ApproverDecision.Pending, label: 'pending' },
      { state: ApproverDecision.Rejected, label: 'rejected' },
    ]
      .map(({ state, label }) => ({
        count: approvers.filter((approver) => approver.decision === state)
          .length,
        label,
      }))
      .filter(({ count }) => count > 0);
  });
  public readonly canReassign = computed(
    () =>
      !this.canToggle() &&
      this.submission().status === SubmissionStatus.Pending &&
      this.isViewerThePublisher(),
  );
  public readonly isViewerThePublisher = computed(() => {
    const sheet = this._activeSheetService.specSheet();
    const user = this._userService.user();
    if (!sheet || !user) {
      return false;
    }

    return this.submission().submittedByUserId === user.id;
  });

  private readonly _activeSheetService = inject(ActiveSheetService);
  private readonly _userService = inject(UserService);
  private readonly _dialog = inject(Dialog);
  private readonly _injector = inject(Injector);

  public async reassignApprovers(): Promise<void> {
    this._dialog.open<unknown, SheetSubmission, ReassignApproversDialog>(
      ReassignApproversDialog,
      {
        injector: this._injector,
        data: this.submission(),
        minWidth: '40%',
      },
    );
  }
}
