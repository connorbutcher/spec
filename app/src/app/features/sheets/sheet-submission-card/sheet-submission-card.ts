import { Component, computed, inject, input, output } from '@angular/core';
import { CurrentUserStore } from '../../../core/auth/current-user.store';
import { SheetSubmission } from '../../approvals/models/sheet-submission.model';
import { SubmissionActions } from '../../approvals/submission-actions/submission-actions';
import { SubmissionApprovers } from '../../approvals/submission-approvers/submission-approvers';
import { SubmissionLabelsUtil } from '../../approvals/submission-labels.util';

/**
 * One submission, stacked in a few short lines for the approvals popover: who submitted it, what it
 * changes and when, the note, the approvers' decisions, and what the user can do. Approve and Reject show for an approver who hasn't
 * decided yet, Withdraw for the submitter.
 */
@Component({
  selector: 'app-sheet-submission-card',
  imports: [SubmissionActions, SubmissionApprovers],
  templateUrl: './sheet-submission-card.html',
  styleUrl: './sheet-submission-card.scss',
})

export class SheetSubmissionCard {
  public readonly submission = input.required<SheetSubmission>();

  /** Offer a link to the changes shown on the sheet; left out while they already are. */
  public readonly showReview = input(true);

  public readonly decided = output<SheetSubmission>();

  public readonly changes = computed(() => SubmissionLabelsUtil.changesLabel(this.submission().changes));

  public readonly submitted = computed(() => SubmissionLabelsUtil.submittedLabel(this.submission()));

  public readonly canDecide = computed(() => {
    const me = this.currentUser.user()?.id;
    return this.submission().approvers.some((approver) => approver.userId === me && approver.decision === 'Pending');
  });

  public readonly canWithdraw = computed(() => {
    const me = this.currentUser.user()?.id;
    return me !== undefined && this.submission().submittedByUserId === me;
  });

  private readonly currentUser = inject(CurrentUserStore);
}
