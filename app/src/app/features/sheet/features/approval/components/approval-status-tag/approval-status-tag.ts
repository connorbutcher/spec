import { Component, computed, input } from '@angular/core';
import { Tag } from 'primeng/tag';
import { ApproverDecision } from '../../../../../../core/types/sheets/approver-decision.enum';
import { SubmissionStatus } from '../../../../../../core/types/sheets/submission-status.enum';
import { ApprovalStatusPipe } from '../../pipes/approval-status-pipe';

@Component({
  selector: 'app-approval-status-tag',
  imports: [Tag, ApprovalStatusPipe],
  templateUrl: './approval-status-tag.html',
  styleUrl: './approval-status-tag.scss',
})
export class ApprovalStatusTag {
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
