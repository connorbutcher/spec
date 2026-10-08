import { Component, computed, inject } from '@angular/core';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { SheetPresenceUser } from '../models/sheet-presence-user.model';
import { SheetLiveStore } from '../sheet-live.store';
import { SheetPresenceBadge } from '../sheet-presence-badge/sheet-presence-badge';
import { checkedOutRowCounts, viewerFirst } from '../sheet-presence.util';
import { SheetStore } from '../sheet.store';

/**
 * Who has the sheet open right now, the viewer first, each with how many rows they have checked out.
 * Warns when the live connection is down, because other people's checkouts stop arriving then.
 */
@Component({
  selector: 'app-sheet-presence',
  imports: [SheetPresenceBadge, TagModule, TooltipModule],
  templateUrl: './sheet-presence.html',
  styleUrl: './sheet-presence.scss',
})
export class SheetPresence {
  public readonly viewerId = computed(() => this.live.viewerId());

  public readonly users = computed<SheetPresenceUser[]>(() =>
    viewerFirst(this.live.users(), this.viewerId()),
  );

  /** Rows checked out, by the id of the user they are checked out to. */
  public readonly checkedOutRows = computed(() =>
    checkedOutRowCounts(this.store.index().rows.values()),
  );

  /** Set while changes are not arriving live. */
  public readonly offlineLabel = computed(() => {
    switch (this.live.connectionState()) {
      case 'connected': {
        return null;
      }
      case 'disconnected': {
        return 'Live updates off';
      }
      default: {
        return 'Connecting…';
      }
    }
  });

  private readonly live = inject(SheetLiveStore);
  private readonly store = inject(SheetStore);
}
