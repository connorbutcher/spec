import { Component, computed, inject, input, output } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { Popover, PopoverModule } from 'primeng/popover';
import { ApprovalsStore } from '../approvals.store';
import { SheetSubmission } from '../models/sheet-submission.model';
import { SubmissionRejectForm } from '../submission-reject-form/submission-reject-form';
import { SubmissionLabelsUtil } from '../submission-labels.util';

/**
 * What can be done with a pending submission: open its changes on the sheet, approve or reject it (for one
 * of its approvers), or withdraw it (for its submitter). Rejecting asks for the reason in a popover.
 */
@Component({
  selector: 'app-submission-actions',
  imports: [ButtonModule, PopoverModule, SubmissionRejectForm],
  templateUrl: './submission-actions.html',
  styleUrl: './submission-actions.scss',
})

export class SubmissionActions {
  public readonly submission = input.required<SheetSubmission>();

  /** Offer Approve and Reject: the viewer is one of its approvers and hasn't decided yet. */
  public readonly canDecide = input(false);

  /** Offer Withdraw: the viewer submitted it. */
  public readonly canWithdraw = input(false);

  /** Offer a link to the changes on the sheet; left out on the sheet itself. */
  public readonly showReview = input(true);

  /** A decision went through. */
  public readonly decided = output<SheetSubmission>();

  public readonly isPending = computed(() => this.submission().status === 'Pending');

  public readonly isBusy = computed(() => this.store.busyId() === this.submission().id);

  private readonly store = inject(ApprovalsStore);
  private readonly router = inject(Router);

  /** Opens the sheet showing the submission's changes over what is published. */
  public review(): void {
    const submission = this.submission();
    void this.router.navigate(SubmissionLabelsUtil.reviewLink(submission), { queryParams: { review: submission.id } });
  }

  public async approve(): Promise<void> {
    this.settle(await this.store.approve(this.submission().id));
  }

  public async reject(reason: string, popover: Popover): Promise<void> {
    const result = await this.store.reject(this.submission().id, reason);
    popover.hide();
    this.settle(result);
  }

  public async withdraw(): Promise<void> {
    this.settle(await this.store.withdraw(this.submission().id));
  }

  private settle(result: SheetSubmission | null): void {
    if (result !== null) {
      this.decided.emit(result);
    }
  }
}
