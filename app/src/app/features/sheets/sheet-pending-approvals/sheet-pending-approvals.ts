import { Component, computed, inject } from '@angular/core';
import { ErrorMessage } from '../../../shared/components/error-message/error-message';
import { ApprovalsStore } from '../../approvals/approvals.store';
import { SheetSubmission } from '../../approvals/models/sheet-submission.model';
import { SheetSubmissionLine } from '../sheet-submission-line/sheet-submission-line';
import { SheetStore } from '../sheet.store';

/**
 * Above the live sheet: each change to this sheet waiting for the user's approval, one line each, so they
 * can approve or reject it where they work, or open it to see the changes in place. Hidden when nothing
 * on the sheet waits for them. A decision reaches everyone with the sheet open over its live connection.
 */
@Component({
  selector: 'app-sheet-pending-approvals',
  imports: [ErrorMessage, SheetSubmissionLine],
  templateUrl: './sheet-pending-approvals.html',
  styleUrl: './sheet-pending-approvals.scss',
  host: {
    '[class.empty]': 'waiting().length === 0',
  },
})

export class SheetPendingApprovals {
  public readonly approvals = inject(ApprovalsStore);

  public readonly waiting = computed<SheetSubmission[]>(() => {
    const sheetId = this.store.sheet()?.id;
    return this.approvals.awaitingMe().filter((submission) => submission.sheetId === sheetId);
  });

  private readonly store = inject(SheetStore);
}
