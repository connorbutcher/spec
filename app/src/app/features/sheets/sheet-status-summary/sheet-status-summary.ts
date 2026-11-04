import { Component, computed, inject } from '@angular/core';
import { ItemStatusUtil } from '../status/item-status.util';
import { PublishScopeUtil } from '../actions/publish-scope.util';
import { SheetLabelsUtil } from '../status/sheet-labels.util';
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
  public readonly icon = ItemStatusUtil.stateIcon('Draft');

  public readonly mine = computed(() => {
    const count = this.store.sheet()?.myDraftCount ?? 0;
    return count > 0 ? `${SheetLabelsUtil.draftCountLabel(count)} by you` : null;
  });

  public readonly theirs = computed(() => PublishScopeUtil.otherDraftsSummary(this.store.sheet()?.otherDrafts ?? []));

  private readonly store = inject(SheetStore);
}
