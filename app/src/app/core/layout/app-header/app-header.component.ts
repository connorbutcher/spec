import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiStatusComponent } from '../api-status/api-status.component';
import { LayoutService } from '../layout.service';

@Component({
  selector: 'app-header',
  imports: [RouterLink, ApiStatusComponent],
  templateUrl: './app-header.component.html',
  styleUrl: './app-header.component.scss',
})
export class AppHeaderComponent {
  private readonly layout = inject(LayoutService);

  public sideNavCollapsed(): boolean {
    return this.layout.sideNavCollapsed();
  }

  public toggleSideNav(): void {
    this.layout.toggleSideNav();
  }
}
