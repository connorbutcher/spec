import { Component, computed, input } from '@angular/core';
import { Tag } from 'primeng/tag';
import { WorkflowState } from '../../../../../../core/models/domain/enums/workflow-state';
import { WorkflowStatePipePipe } from '../../pipes/workflow-state-pipe';

@Component({
  selector: 'app-workflow-step-status-tag',
  imports: [Tag, WorkflowStatePipePipe],
  templateUrl: './workflow-step-status-tag.html',
  styleUrl: './workflow-step-status-tag.scss',
})
export class WorkflowStepStatusTag {
  public readonly state = input.required<WorkflowState>();

  public readonly severity = computed(() => {
    switch (this.state()) {
      case WorkflowState.Approved:
        return 'success';
      case WorkflowState.Rejected:
        return 'danger';
      case WorkflowState.Pending:
      default:
        return 'secondary';
    }
  });
}
