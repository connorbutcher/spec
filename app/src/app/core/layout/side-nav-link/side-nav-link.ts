import { Component, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { TooltipModule } from 'primeng/tooltip';
import { NavItem } from '../side-nav/nav-item.model';

/** A single side nav entry. Shows only its icon (with a tooltip label) when the nav is collapsed. */
@Component({
  selector: 'app-side-nav-link',
  imports: [RouterLink, RouterLinkActive, TooltipModule],
  templateUrl: './side-nav-link.html',
  styleUrl: './side-nav-link.scss',
})
export class SideNavLink {
  public readonly item = input.required<NavItem>();
  public readonly collapsed = input(false);
}
