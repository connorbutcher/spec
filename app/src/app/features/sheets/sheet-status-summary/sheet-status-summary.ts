import { Component, computed, inject } from '@angular/core';
import { TooltipModule } from 'primeng/tooltip';
import { ItemStatusUtil } from '../status/item-status.util';
import { PublishScopeUtil } from '../actions/publish-scope.util';
import { SheetLabelsUtil } from '../status/sheet-labels.util';
import { SheetStore } from '../sheet.store';

/** A number of drafts, and the same in words for the tooltip, e.g. 2 and "2 drafts by A. Smith". */
type DraftCount = {
  count: number;
  text: string;
}

/**
 * What is waiting to be published on the whole sheet, in the toolbar: the viewer's drafts and other
 * people's, each as an icon and a number, with who and how many in words on hover. It uses the icon and
 * colours of the status column, so a count here and the rows it counts look alike. Nothing is shown when
 * nothing is waiting.
 */
@Component({
  selector: 'app-sheet-status-summary',
  imports: [TooltipModule],
  templateUrl: './sheet-status-summary.html',
  styleUrl: './sheet-status-summary.scss',
  host: {
    role: 'status',
    'aria-label': 'Waiting to be published',
  },
})

export class SheetStatusSummary {
  public readonly icon = ItemStatusUtil.stateIcon('Draft');

  public readonly mine = computed<DraftCount | null>(() => {
    const count = this.store.sheet()?.myDraftCount ?? 0;
    return count > 0 ? { count, text: `${SheetLabelsUtil.draftCountLabel(count)} by you` } : null;
  });

  public readonly theirs = computed<DraftCount | null>(() => {
    const others = this.store.sheet()?.otherDrafts ?? [];
    const text = PublishScopeUtil.otherDraftsSummary(others);
    return text === null ? null : { count: PublishScopeUtil.otherDraftCount(others), text };
  });

  private readonly store = inject(SheetStore);
}
