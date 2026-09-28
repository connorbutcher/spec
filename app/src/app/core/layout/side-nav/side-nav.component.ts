import { Component, inject } from '@angular/core';
import { LayoutService } from '../layout.service';
import { FOOTER_NAV_ITEMS, MAIN_NAV_ITEMS } from './nav-items';
import { SideNavLinkComponent } from './side-nav-link.component';

@Component({
  selector: 'app-side-nav',
  imports: [SideNavLinkComponent],
  templateUrl: './side-nav.component.html',
  styleUrl: './side-nav.component.scss',
  host: {
    '[class.collapsed]': 'collapsed()',
  },
})
export class SideNavComponent {
  public readonly mainItems = MAIN_NAV_ITEMS;
  public readonly footerItems = FOOTER_NAV_ITEMS;

  private readonly layout = inject(LayoutService);

  public collapsed(): boolean {
    return this.layout.sideNavCollapsed();
  }
}
