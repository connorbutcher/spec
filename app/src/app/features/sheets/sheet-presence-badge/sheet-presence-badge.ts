import { Component, computed, input } from '@angular/core';
import { AvatarModule } from 'primeng/avatar';
import { OverlayBadgeModule } from 'primeng/overlaybadge';
import { TooltipModule } from 'primeng/tooltip';
import { SheetPresenceUser } from '../models/sheet-presence-user.model';

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

  public readonly initials = computed(() =>
    this.user()
      .displayName.split(/\s+/)
      .filter((word) => word !== '')
      .slice(0, 2)
      .map((word) => word[0].toUpperCase())
      .join(''),
  );

  public readonly description = computed(() => {
    const user = this.user();
    const rows = this.checkedOutRows();
    const parts = [this.isMe() ? `${user.displayName} (you)` : user.displayName];
    if (rows > 0) {
      parts.push(`${rows} ${rows === 1 ? 'row' : 'rows'} checked out`);
    }
    if (user.connectionCount > 1) {
      parts.push(`${user.connectionCount} tabs`);
    }
    return parts.join(' · ');
  });
}
