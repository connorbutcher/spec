import { Component, inject } from '@angular/core';
import { ErrorMessage } from '../../../shared/components/error-message/error-message';
import { ApprovalsStore } from '../approvals.store';
import { SubmissionTable } from '../submission-table/submission-table';

/**
 * The Approvals screen: changes waiting for the user's decision, then what the user submitted and where it
 * stands, including why anything was rejected. Opening it reads both lists again.
 */
@Component({
  selector: 'app-approvals-page',
  imports: [ErrorMessage, SubmissionTable],
  templateUrl: './approvals-page.html',
  styleUrl: './approvals-page.scss',
})

export class ApprovalsPage {
  public readonly store = inject(ApprovalsStore);

  constructor() {
    this.store.reload();
  }
}
