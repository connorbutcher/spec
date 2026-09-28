import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiStatus } from '../api-status/api-status';
import { LayoutService } from '../layout.service';

@Component({
  selector: 'app-header',
  imports: [RouterLink, ApiStatus],
  templateUrl: './app-header.html',
  styleUrl: './app-header.scss',
})
export class AppHeader {
  private readonly layout = inject(LayoutService);

  public sideNavCollapsed(): boolean {
    return this.layout.sideNavCollapsed();
  }

  public toggleSideNav(): void {
    this.layout.toggleSideNav();
  }
}
