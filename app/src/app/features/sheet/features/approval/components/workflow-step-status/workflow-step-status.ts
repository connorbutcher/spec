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
import { WorkflowStepActorStatus } from '../workflow-step-actor-status/workflow-step-actor-status';
import { WorkflowStepStatusTag } from '../workflow-step-status-tag/workflow-step-status-tag';
import { SheetSubmission } from '../../../../../../core/models/domain/sheets/sheet';

@Component({
  selector: 'app-workflow-step-status',
  imports: [
    DatePipe,
    WorkflowStepActorStatus,
    WorkflowStepStatusTag,
    ButtonDirective,
    Tooltip,
  ],
  templateUrl: './workflow-step-status.html',
  styleUrl: './workflow-step-status.scss',
})
export class WorkflowStepStatus {
  public readonly workflowStep = input.required<SheetSubmission>();
  public readonly canToggle = input.required<boolean>();
  public readonly heading = input('Current approval');
  public readonly approvers = computed(() => {
    const actors = [...this.workflowStep().approvers];
    if (this.workflowStep().status === SubmissionStatus.Pending) {
      actors.sort(
        (left, right) =>
          Number(right.decision === ApproverDecision.Pending) -
          Number(left.decision === ApproverDecision.Pending),
      );
    }
    return actors;
  });
  public readonly approvalCounts = computed(() => {
    const actors = this.workflowStep().approvers;
    return [
      { state: ApproverDecision.Approved, label: 'approved' },
      { state: ApproverDecision.Pending, label: 'pending' },
      { state: ApproverDecision.Rejected, label: 'rejected' },
    ]
      .map(({ state, label }) => ({
        count: actors.filter((actor) => actor.decision === state).length,
        label,
      }))
      .filter(({ count }) => count > 0);
  });
  public readonly canReassign = computed(
    () =>
      !this.canToggle() &&
      this.workflowStep().status === SubmissionStatus.Pending &&
      this.isViewerThePublisher(),
  );
  public readonly isViewerThePublisher = computed(() => {
    const sheet = this._activeSheetService.specSheet();
    const user = this._userService.user();
    if (!sheet || !user) {
      return false;
    }

    return this.workflowStep().submittedByUserId === user.id;
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
        data: this.workflowStep(),
        minWidth: '40%',
      },
    );
  }
}
