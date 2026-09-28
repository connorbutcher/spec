import { Component, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { NavItem } from './nav-item.model';

/** A single side nav entry. Shows only its icon (with a tooltip label) when the nav is collapsed. */
@Component({
  selector: 'app-side-nav-link',
  imports: [RouterLink, RouterLinkActive],
  template: `
    <a
      class="side-nav-link"
      [routerLink]="item().path"
      routerLinkActive="active"
      ariaCurrentWhenActive="page"
      [attr.title]="collapsed() ? item().label : null"
    >
      <i class="pi" [class]="item().icon" aria-hidden="true"></i>
      <span class="label" [class.visually-hidden]="collapsed()">{{ item().label }}</span>
    </a>
  `,
  styleUrl: './side-nav-link.component.scss',
})
export class SideNavLinkComponent {
  public readonly item = input.required<NavItem>();
  public readonly collapsed = input(false);
}
