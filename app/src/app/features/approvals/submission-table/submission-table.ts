import { Component, input } from '@angular/core';
import { TableModule } from 'primeng/table';
import { SheetSubmission } from '../models/sheet-submission.model';
import { SubmissionActions } from '../submission-actions/submission-actions';
import { SubmissionApprovers } from '../submission-approvers/submission-approvers';
import { SubmissionLabelsUtil } from '../submission-labels.util';

/**
 * Submissions in a compact table: the sheet, who submitted it and when, what it changes, its note, its
 * approvers' decisions and where it stands, with what the viewer can do about it. `role` says whether the
 * viewer is deciding on these (approve, reject) or submitted them (withdraw).
 */
@Component({
  selector: 'app-submission-table',
  imports: [SubmissionActions, SubmissionApprovers, TableModule],
  templateUrl: './submission-table.html',
  styleUrl: './submission-table.scss',
})

export class SubmissionTable {
  public readonly submissions = input.required<SheetSubmission[]>();

  public readonly role = input.required<'approver' | 'submitter'>();

  public readonly emptyMessage = input.required<string>();

  public sheetLabel(submission: SheetSubmission): string {
    return SubmissionLabelsUtil.sheetLabel(submission);
  }

  public submittedLabel(submission: SheetSubmission): string {
    return SubmissionLabelsUtil.submittedLabel(submission);
  }

  public changesLabel(submission: SheetSubmission): string {
    return submission.status === 'Pending' || submission.status === 'Approved'
      ? SubmissionLabelsUtil.changesLabel(submission.changes)
      : '—';
  }

  public statusLabel(submission: SheetSubmission): string {
    return SubmissionLabelsUtil.statusLabel(submission);
  }

  public rejection(submission: SheetSubmission): string | null {
    return SubmissionLabelsUtil.rejection(submission);
  }
}
