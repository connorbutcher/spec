import { Component, computed, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { Popover, PopoverModule } from 'primeng/popover';
import { TooltipModule } from 'primeng/tooltip';
import { ErrorMessage } from '../../../shared/components/error-message/error-message';
import { ApprovalsStore } from '../../approvals/approvals.store';
import { SheetSubmission } from '../../approvals/models/sheet-submission.model';
import { SheetSubmissionCard } from '../sheet-submission-card/sheet-submission-card';
import { SheetStore } from '../sheet.store';

/**
 * Approvals on the sheet's top line, as one small button that only shows when there is something to
 * decide: "2 to approve" on the live sheet, or "Reviewing" while a submission's changes are shown. It
 * opens a popover with each submission's details and Review, Approve and Reject (Withdraw for the
 * submitter). A decision made while reviewing goes back to the live sheet.
 */
@Component({
  selector: 'app-sheet-approvals-menu',
  imports: [ButtonModule, ErrorMessage, PopoverModule, SheetSubmissionCard, TooltipModule],
  templateUrl: './sheet-approvals-menu.html',
  styleUrl: './sheet-approvals-menu.scss',
})

export class SheetApprovalsMenu {
  public readonly approvals = inject(ApprovalsStore);

  public readonly isReviewing = computed(() => (this.store.sheet()?.reviewedSubmissionId ?? null) !== null);

  /** While reviewing, the submission shown; on the live sheet, what on it waits for the user. */
  public readonly submissions = computed<SheetSubmission[]>(() => {
    const sheet = this.store.sheet();
    if (sheet === null) {
      return [];
    }
    if (sheet.reviewedSubmissionId) {
      const reviewed = [...this.approvals.awaitingMe(), ...this.approvals.mine()]
        .find((submission) => submission.id === sheet.reviewedSubmissionId);
      return reviewed === undefined ? [] : [reviewed];
    }
    return this.approvals.awaitingMe().filter((submission) => submission.sheetId === sheet.id);
  });

  public readonly label = computed(() =>
    this.isReviewing() ? 'Reviewing' : `${this.submissions().length} to approve`,
  );

  public readonly tooltip = computed(() =>
    this.isReviewing() ? 'Approve or reject the changes shown' : 'Changes on this sheet waiting for your approval',
  );

  public readonly heading = computed(() =>
    this.isReviewing() ? 'Changes shown for approval' : 'Waiting for your approval',
  );

  private readonly store = inject(SheetStore);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  /** After a decision: nothing left to show while reviewing, so back to live; otherwise close once all are done. */
  public decided(popover: Popover): void {
    if (this.isReviewing()) {
      popover.hide();
      void this.router.navigate([], { relativeTo: this.route, queryParams: { review: null }, queryParamsHandling: 'merge' });
    } else if (this.submissions().length <= 1) {
      popover.hide();
    }
  }
}
