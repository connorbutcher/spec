import { Component, computed, input } from '@angular/core';
import { AvatarModule } from 'primeng/avatar';
import { OverlayBadgeModule } from 'primeng/overlaybadge';
import { TooltipModule } from 'primeng/tooltip';
import { SheetPresenceUser } from '../models/sheet-presence-user.model';
import { initials, presenceDescription } from '../sheet-presence.util';

/**
 * One person who has the sheet open: their initials, with the number of rows checked out to them as a
 * badge. The tooltip spells both out.
 */
@Component({
  selector: 'app-sheet-presence-badge',
  imports: [AvatarModule, OverlayBadgeModule, TooltipModule],
  templateUrl: './sheet-presence-badge.html',
  styleUrl: './sheet-presence-badge.scss',
})
export class SheetPresenceBadge {
  public readonly user = input.required<SheetPresenceUser>();
  public readonly isMe = input(false);
  /** How many rows the viewer can see checked out to this person. */
  public readonly checkedOutRows = input(0);

  public readonly initials = computed(() => initials(this.user().displayName));

  public readonly description = computed(() =>
    presenceDescription(this.user(), this.isMe(), this.checkedOutRows()),
  );
}
