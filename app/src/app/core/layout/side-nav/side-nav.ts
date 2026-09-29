import { Component, inject } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { MenuModule } from 'primeng/menu';
import { SideNavLink } from '../side-nav-link/side-nav-link';
import { LayoutService } from '../layout.service';
import { NavItem } from './nav-item.model';
import { FOOTER_NAV_ITEMS, MAIN_NAV_ITEMS } from './nav-items';

/** The main navigation: PrimeNG menus of the app's sections, collapsible to icons only. */
@Component({
  selector: 'app-side-nav',
  imports: [MenuModule, SideNavLink],
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
