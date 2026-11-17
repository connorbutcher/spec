import { Dialog } from '@angular/cdk/dialog';
import { DatePipe } from '@angular/common';
import { Component, computed, inject, Injector, input } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { Tooltip } from 'primeng/tooltip';
import { WorkflowState } from '../../../../../../core/models/domain/enums/workflow-state';
import { WorkflowStep } from '../../../../../../core/models/domain/workflow/workflow-step';
import { UserService } from '../../../../../../core/services/user.service';
import { ReassignApproversDialog } from '../../../../dialogs/reassign-approvers-dialog/reassign-approvers-dialog';
import { ActiveSheetService } from '../../../../services/active-sheet.service';
import { WorkflowStepActorStatus } from '../workflow-step-actor-status/workflow-step-actor-status';
import { WorkflowStepStatusTag } from '../workflow-step-status-tag/workflow-step-status-tag';

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
  public readonly workflowStep = input.required<WorkflowStep>();
  public readonly canToggle = input.required<boolean>();
  public readonly heading = input('Current approval');
  public readonly approvers = computed(() => {
    const actors = [...this.workflowStep().actors];
    if (this.workflowStep().state === WorkflowState.Pending) {
      actors.sort(
        (left, right) =>
          Number(right.state === WorkflowState.Pending) -
          Number(left.state === WorkflowState.Pending),
      );
    }
    return actors;
  });
  public readonly approvalCounts = computed(() => {
    const actors = this.workflowStep().actors;
    return [
      { state: WorkflowState.Approved, label: 'approved' },
      { state: WorkflowState.Pending, label: 'pending' },
      { state: WorkflowState.Rejected, label: 'rejected' },
    ]
      .map(({ state, label }) => ({
        count: actors.filter((actor) => actor.state === state).length,
        label,
      }))
      .filter(({ count }) => count > 0);
  });
  public readonly canReassign = computed(
    () =>
      !this.canToggle() &&
      this.workflowStep().state === WorkflowState.Pending &&
      this.isViewerThePublisher(),
  );
  public readonly isViewerThePublisher = computed(() => {
    const sheet = this._activeSheetService.specSheet();
    const user = this._userService.user();
    if (!sheet || !user) {
      return false;
    }

    return sheet.specSheet.publishedByUser?.id === user.id;
  });

  private readonly _activeSheetService = inject(ActiveSheetService);
  private readonly _userService = inject(UserService);
  private readonly _dialog = inject(Dialog);
  private readonly _injector = inject(Injector);

  public async reassignApprovers(): Promise<void> {
    this._dialog.open<unknown, WorkflowStep, ReassignApproversDialog>(
      ReassignApproversDialog,
      {
        injector: this._injector,
        data: this.workflowStep(),
        minWidth: '40%',
      },
    );
  }
}
