import { Component, inject } from '@angular/core';
import { LayoutService } from '../layout.service';
import { FOOTER_NAV_ITEMS, MAIN_NAV_ITEMS } from './nav-items';
import { SideNavLink } from '../side-nav-link/side-nav-link';

@Component({
  selector: 'app-side-nav',
  imports: [SideNavLink],
  templateUrl: './side-nav.html',
  styleUrl: './side-nav.scss',
  host: {
    '[class.collapsed]': 'collapsed()',
  },
})
export class SideNav {
  public readonly mainItems = MAIN_NAV_ITEMS;
  public readonly footerItems = FOOTER_NAV_ITEMS;

  private readonly layout = inject(LayoutService);

  public collapsed(): boolean {
    return this.layout.sideNavCollapsed();
  }
}
