import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { DateTime } from 'luxon';
import { ButtonDirective } from 'primeng/button';
import { SubmissionStatus } from '../../../../../core/types/sheets/submission-status.enum';
import { ISidePanelLayer } from '../../../../side-panel/models/side-panel-layer-data.interface';
import { SidePanelService } from '../../../../side-panel/side-panel.service';
import { ActiveSheetService } from '../../../services/active-sheet.service';
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

  public readonly activeSubmission = computed(() =>
    this._submissions().find(
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
  public readonly displayedSubmission = computed(
    () => this.activeSubmission() ?? this.pastSubmissions()[0],
  );
  public readonly historySubmissions = computed(() =>
    this.pastSubmissions().filter(
      (attempt) => attempt.id !== this.displayedSubmission()?.id,
    ),
  );

  public close(): void {
    this._sidePanelService.pop();
  }
}
