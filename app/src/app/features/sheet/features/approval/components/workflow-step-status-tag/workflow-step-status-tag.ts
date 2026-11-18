import { Component, computed, input } from '@angular/core';
import { Tag } from 'primeng/tag';
import { ApproverDecision } from '../../../../../../core/types/sheets/approver-decision.enum';
import { SubmissionStatus } from '../../../../../../core/types/sheets/submission-status.enum';
import { WorkflowStatePipePipe } from '../../pipes/workflow-state-pipe';

@Component({
  selector: 'app-workflow-step-status-tag',
  imports: [Tag, WorkflowStatePipePipe],
  templateUrl: './workflow-step-status-tag.html',
  styleUrl: './workflow-step-status-tag.scss',
})
export class WorkflowStepStatusTag {
  public readonly state = input.required<SubmissionStatus | ApproverDecision>();

  public readonly severity = computed(() => {
    switch (this.state()) {
      case SubmissionStatus.Approved:
        return 'success';
      case SubmissionStatus.Rejected:
        return 'danger';
      case SubmissionStatus.Pending:
      default:
        return 'secondary';
    }
  });
}
