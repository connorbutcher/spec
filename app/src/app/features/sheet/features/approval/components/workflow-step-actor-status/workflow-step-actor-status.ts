import { DatePipe } from '@angular/common';
import { Component, computed, inject, input } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { WorkflowStepActor } from '../../../../../../core/models/domain/workflow/workflow-step-actor';
import { WorkflowStepStatusTag } from '../workflow-step-status-tag/workflow-step-status-tag';

@Component({
  selector: 'app-workflow-step-actor-status',
  imports: [DatePipe, WorkflowStepStatusTag],
  templateUrl: './workflow-step-actor-status.html',
  styleUrl: './workflow-step-actor-status.scss',
})
export class WorkflowStepActorStatus {
  public readonly actor = input.required<WorkflowStepActor>();
  public readonly safeComment = computed(() => {
    const actor = this.actor();
    if (!actor.comment) {
      return undefined;
    }

    return this._domSanitizer.bypassSecurityTrustHtml(actor.comment);
  });

  private readonly _domSanitizer = inject(DomSanitizer);
}
