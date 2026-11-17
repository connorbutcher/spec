import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { DateTime } from 'luxon';
import { ButtonDirective } from 'primeng/button';
import { WorkflowState } from '../../../../../core/models/domain/enums/workflow-state';
import { ISidePanelLayer } from '../../../../side-panel/models/side-panel-layer-data.interface';
import { SidePanelService } from '../../../../side-panel/side-panel.service';
import { ActiveSheetService } from '../../../services/active-sheet.service';
import { WorkflowStepStatusTag } from '../components/workflow-step-status-tag/workflow-step-status-tag';
import { WorkflowStepStatus } from '../components/workflow-step-status/workflow-step-status';

@Component({
  selector: 'app-approval-side-panel',
  imports: [
    ButtonDirective,
    DatePipe,
    WorkflowStepStatus,
    WorkflowStepStatusTag,
  ],
  templateUrl: './approval-side-panel.html',
  styleUrl: './approval-side-panel.scss',
})
export class ApprovalSidePanel
  implements ISidePanelLayer<Record<string, never>>
{
  public data: Record<string, never> = {};
  public readonly title = signal('Approval Status');

  private readonly _activeSheetService = inject(ActiveSheetService);
  private readonly _sidePanelService = inject(SidePanelService);
  private readonly _workflowAttempts = computed(
    () => this._activeSheetService.specSheet().specSheet.workflowSteps,
  );

  public readonly activeWorkflowAttempt = computed(() =>
    this._workflowAttempts().find(
      (attempt) => attempt.state === WorkflowState.Pending,
    ),
  );
  public readonly pastWorkflowAttempts = computed(() =>
    this._workflowAttempts()
      .filter((attempt) => attempt.state !== WorkflowState.Pending)
      .sort(
        (left, right) =>
          DateTime.fromISO(right.startedDateTime).toMillis() -
          DateTime.fromISO(left.startedDateTime).toMillis(),
      ),
  );
  public readonly displayedWorkflowAttempt = computed(
    () => this.activeWorkflowAttempt() ?? this.pastWorkflowAttempts()[0],
  );
  public readonly historyWorkflowAttempts = computed(() =>
    this.pastWorkflowAttempts().filter(
      (attempt) => attempt.id !== this.displayedWorkflowAttempt()?.id,
    ),
  );

  public close(): void {
    this._sidePanelService.pop();
  }
}
