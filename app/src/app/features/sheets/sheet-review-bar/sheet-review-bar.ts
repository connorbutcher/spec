import { Component, computed, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CurrentUserStore } from '../../../core/auth/current-user.store';
import { ErrorMessage } from '../../../shared/components/error-message/error-message';
import { ApprovalsStore } from '../../approvals/approvals.store';
import { SheetSubmission } from '../../approvals/models/sheet-submission.model';
import { SubmissionActions } from '../../approvals/submission-actions/submission-actions';
import { SubmissionApprovers } from '../../approvals/submission-approvers/submission-approvers';
import { SubmissionLabelsUtil } from '../../approvals/submission-labels.util';
import { SheetStore } from '../sheet.store';

/**
 * Above a sheet showing a submission's changes for review: whose changes they are, what they change and
 * the note, the approvers' decisions, and Approve and Reject for an approver who hasn't decided (Withdraw
 * for the submitter). A decision, or Back to live, goes back to the live sheet.
 */
@Component({
  selector: 'app-sheet-review-bar',
  imports: [ButtonModule, ErrorMessage, SubmissionActions, SubmissionApprovers],
  templateUrl: './sheet-review-bar.html',
  styleUrl: './sheet-review-bar.scss',
})

export class SheetReviewBar {
  public readonly approvals = inject(ApprovalsStore);

  /** The submission being reviewed, from the user's lists: waiting for them, or theirs. */
  public readonly submission = computed<SheetSubmission | null>(() => {
    const id = this.store.sheet()?.reviewedSubmissionId ?? null;
    if (id === null) {
      return null;
    }
    return [...this.approvals.awaitingMe(), ...this.approvals.mine()].find((candidate) => candidate.id === id) ?? null;
  });

  public readonly changes = computed(() => {
    const submission = this.submission();
    return submission === null ? '' : SubmissionLabelsUtil.changesLabel(submission.changes);
  });

  public readonly canDecide = computed(() => {
    const me = this.currentUser.user()?.id;
    return this.submission()?.approvers.some((approver) => approver.userId === me && approver.decision === 'Pending') === true;
  });

  public readonly canWithdraw = computed(() => {
    const me = this.currentUser.user()?.id;
    return me !== undefined && this.submission()?.submittedByUserId === me;
  });

  private readonly store = inject(SheetStore);
  private readonly currentUser = inject(CurrentUserStore);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  /** Drops `?review=` from the address, which puts the sheet back to live. */
  public backToLive(): void {
    void this.router.navigate([], { relativeTo: this.route, queryParams: { review: null }, queryParamsHandling: 'merge' });
  }
}
