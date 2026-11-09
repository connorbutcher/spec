import { Component, computed, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { BadgeModule } from 'primeng/badge';
import { TooltipModule } from 'primeng/tooltip';
import { NavItem } from '../side-nav/nav-item.model';

/**
 * A single side nav entry. Shows only its icon (with a tooltip label) when the nav is collapsed. A `count`
 * above zero shows as a badge, such as the changes waiting for the user's approval.
 */
@Component({
  selector: 'app-side-nav-link',
  imports: [BadgeModule, RouterLink, RouterLinkActive, TooltipModule],
  templateUrl: './side-nav-link.html',
  styleUrl: './side-nav-link.scss',
})
export class SideNavLink {
  public readonly item = input.required<NavItem>();
  public readonly collapsed = input(false);
  public readonly count = input(0);

  public readonly label = computed(() => {
    const count = this.count();
    return count > 0 ? `${this.item().label} (${count} waiting)` : this.item().label;
  });
}
