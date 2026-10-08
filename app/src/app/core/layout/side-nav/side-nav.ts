import { Component, inject } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { MenuModule } from 'primeng/menu';
import { PhasesStore } from '../../../features/phases/phases.store';
import { ApiStatus } from '../api-status/api-status';
import { SideNavBanner } from '../side-nav-banner/side-nav-banner';
import { SideNavLink } from '../side-nav-link/side-nav-link';
import { SideNavPhaseTree } from '../side-nav-phase-tree/side-nav-phase-tree';
import { LayoutService } from '../layout.service';
import { NavItem } from './nav-item.model';
import { FOOTER_NAV_ITEMS, MAIN_NAV_ITEMS } from './nav-items';

/**
 * The main navigation: the app name, then PrimeNG menus of the app's sections, collapsible to icons
 * only. While the Phases section is open its phase tree shows under the Phases entry. The API status
 * sits at the bottom.
 */
@Component({
  selector: 'app-side-nav',
  imports: [MenuModule, ApiStatus, SideNavBanner, SideNavLink, SideNavPhaseTree],
  templateUrl: './side-nav.html',
  styleUrl: './side-nav.scss',
  host: {
    '[class.collapsed]': 'collapsed()',
  },
})
export class SideNav {
  public readonly mainItems = MAIN_NAV_ITEMS.map(toMenuItem);
  public readonly footerItems = FOOTER_NAV_ITEMS.map(toMenuItem);

  private readonly layout = inject(LayoutService);
  private readonly phases = inject(PhasesStore);

  /** Whether the current page is in the Phases section, which is when its tree shows. */
  public inPhases(): boolean {
    return this.phases.inPhasesSection();
  }

  public collapsed(): boolean {
    return this.layout.sideNavCollapsed();
  }

  /** The nav entry a menu item was built from, for the item template. */
  public navItem(item: MenuItem): NavItem {
    return item['navItem'] as NavItem;
  }
}

function toMenuItem(navItem: NavItem): MenuItem {
  return { label: navItem.label, icon: `pi ${navItem.icon}`, routerLink: navItem.path, navItem };
}
