import { Component, input } from '@angular/core';
import { TooltipModule } from 'primeng/tooltip';
import { SubmissionApprover } from '../models/submission-approver.model';
import { SubmissionLabelsUtil } from '../submission-labels.util';

/** A submission's approvers, each with an icon for what they decided and, on hover, the reason for a rejection. */
@Component({
  selector: 'app-submission-approvers',
  imports: [TooltipModule],
  templateUrl: './submission-approvers.html',
  styleUrl: './submission-approvers.scss',
})

export class SubmissionApprovers {
  public readonly approvers = input.required<SubmissionApprover[]>();

  public icon(approver: SubmissionApprover): string {
    return SubmissionLabelsUtil.decisionIcon(approver.decision);
  }
}
