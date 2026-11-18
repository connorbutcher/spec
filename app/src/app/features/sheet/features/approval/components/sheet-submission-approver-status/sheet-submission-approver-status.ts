import { DatePipe } from '@angular/common';
import { Component, computed, inject, input } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { ApprovalStatusTag } from '../approval-status-tag/approval-status-tag';
import { SheetSubmissionApprover } from '../../../../../../core/models/domain/sheets/sheet-submission-approver';

@Component({
  selector: 'app-sheet-submission-approver-status',
  imports: [DatePipe, ApprovalStatusTag],
  templateUrl: './sheet-submission-approver-status.html',
  styleUrl: './sheet-submission-approver-status.scss',
})
export class SheetSubmissionApproverStatus {
  public readonly approver = input.required<SheetSubmissionApprover>();
  public readonly safeComment = computed(() => {
    const approver = this.approver();
    if (!approver.reason) {
      return undefined;
    }

    return this._domSanitizer.bypassSecurityTrustHtml(approver.reason);
  });

  private readonly _domSanitizer = inject(DomSanitizer);
}
