import { Component, computed, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ErrorMessage } from '../../../shared/components/error-message/error-message';
import { ApprovalsStore } from '../../approvals/approvals.store';
import { SheetSubmission } from '../../approvals/models/sheet-submission.model';
import { SheetSubmissionLine } from '../sheet-submission-line/sheet-submission-line';
import { SheetStore } from '../sheet.store';

/**
 * Above a sheet showing a submission's changes for review: the submission, with Approve and Reject for an
 * approver who hasn't decided (Withdraw for the submitter). A decision goes back to the live sheet, as
 * does Back to live in the view menu.
 */
@Component({
  selector: 'app-sheet-review-bar',
  imports: [ErrorMessage, SheetSubmissionLine],
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

  private readonly store = inject(SheetStore);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  /** Drops `?review=` from the address, which puts the sheet back to live. */
  public backToLive(): void {
    void this.router.navigate([], { relativeTo: this.route, queryParams: { review: null }, queryParamsHandling: 'merge' });
  }
}
