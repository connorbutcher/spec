import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { DateTime } from 'luxon';
import { ButtonDirective } from 'primeng/button';
import { SubmissionStatus } from '../../../../../core/types/sheets/submission-status.enum';
import { ISidePanelLayer } from '../../../../side-panel/models/side-panel-layer-data.interface';
import { SidePanelService } from '../../../../side-panel/side-panel.service';
import { ActiveSheetService } from '../../../services/active-sheet.service';
import { SheetSubmissionService } from '../../../services/sheet-submission.service';
import { ApprovalStatusTag } from '../components/approval-status-tag/approval-status-tag';
import { SheetSubmissionStatus } from '../components/sheet-submission-status/sheet-submission-status';

@Component({
  selector: 'app-approval-side-panel',
  imports: [
    ButtonDirective,
    DatePipe,
    SheetSubmissionStatus,
    ApprovalStatusTag,
  ],
  templateUrl: './approval-side-panel.html',
  styleUrl: './approval-side-panel.scss',
})
export class ApprovalSidePanel implements ISidePanelLayer<
  Record<string, never>
> {
  public data: Record<string, never> = {};
  public readonly title = signal('Approval Status');

  private readonly _activeSheetService = inject(ActiveSheetService);
  private readonly _sidePanelService = inject(SidePanelService);
  private readonly _submissions = computed(
    () => this._activeSheetService.specSheet().submissions,
  );

  /** Every submission awaiting approval: each is one user's checked-out changes. */
  public readonly activeSubmissions = computed(() =>
    this._submissions().filter(
      (attempt) => attempt.status === SubmissionStatus.Pending,
    ),
  );
  public readonly pastSubmissions = computed(() =>
    this._submissions()
      .filter((attempt) => attempt.status !== SubmissionStatus.Pending)
      .sort(
        (left, right) =>
          DateTime.fromISO(right.submittedDateTime).toMillis() -
          DateTime.fromISO(left.submittedDateTime).toMillis(),
      ),
  );
  public readonly whoseChanges = SheetSubmissionService.whoseChanges;

  public close(): void {
    this._sidePanelService.pop();
  }
}
