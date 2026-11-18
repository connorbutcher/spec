import { DatePipe } from '@angular/common';
import { Component, computed, inject, input } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { WorkflowStepStatusTag } from '../workflow-step-status-tag/workflow-step-status-tag';
import { SheetSubmissionApprover } from '../../../../../../core/models/domain/sheets/sheet';

@Component({
  selector: 'app-workflow-step-actor-status',
  imports: [DatePipe, WorkflowStepStatusTag],
  templateUrl: './workflow-step-actor-status.html',
  styleUrl: './workflow-step-actor-status.scss',
})
export class WorkflowStepActorStatus {
  public readonly actor = input.required<SheetSubmissionApprover>();
  public readonly safeComment = computed(() => {
    const actor = this.actor();
    if (!actor.reason) {
      return undefined;
    }

    return this._domSanitizer.bypassSecurityTrustHtml(actor.reason);
  });

  private readonly _domSanitizer = inject(DomSanitizer);
}
