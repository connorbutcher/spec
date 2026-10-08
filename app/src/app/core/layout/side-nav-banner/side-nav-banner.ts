import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { LayoutService } from '../layout.service';

/**
 * The top of the side nav: the app's name, linking home, and the button that collapses the nav. When
 * the nav is collapsed the name shrinks to its icon (with a tooltip) above the button.
 */
@Component({
  selector: 'app-side-nav-banner',
  imports: [ButtonModule, RouterLink, TooltipModule],
  templateUrl: './side-nav-banner.html',
  styleUrl: './side-nav-banner.scss',
  host: {
    '[class.collapsed]': 'collapsed()',
  },
})
export class SideNavBanner {
  public readonly appName = 'PU Spec Sheet';

  private readonly layout = inject(LayoutService);

  public collapsed(): boolean {
    return this.layout.sideNavCollapsed();
  }

  public toggle(): void {
    this.layout.toggleSideNav();
  }
}
