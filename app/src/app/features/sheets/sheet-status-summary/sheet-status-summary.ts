import { Component, computed, inject } from '@angular/core';
import { stateIcon } from '../item-status.util';
import { otherDraftsSummary } from '../publish-scope.util';
import { draftCountLabel } from '../sheet-labels.util';
import { SheetStore } from '../sheet.store';

/**
 * What is waiting to be published on the whole sheet, in the toolbar: how many drafts are the viewer's
 * and how many are other people's, named. It uses the icon and colours of the status column, so a count
 * here and the rows it counts look alike. Nothing is shown when nothing is waiting.
 */
@Component({
  selector: 'app-sheet-status-summary',
  templateUrl: './sheet-status-summary.html',
  styleUrl: './sheet-status-summary.scss',
  host: {
    role: 'status',
    'aria-label': 'Waiting to be published',
  },
})
export class SheetStatusSummary {
  public readonly icon = stateIcon('Draft');

  public readonly mine = computed(() => {
    const count = this.store.sheet()?.myDraftCount ?? 0;
    return count > 0 ? `${draftCountLabel(count)} by you` : null;
  });

  public readonly theirs = computed(() => otherDraftsSummary(this.store.sheet()?.otherDrafts ?? []));

  private readonly store = inject(SheetStore);
}
