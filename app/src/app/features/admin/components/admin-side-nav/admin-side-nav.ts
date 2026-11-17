import { Component, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AdminNavItem } from '../../models/admin-nav-item.class';

@Component({
  selector: 'app-admin-side-nav',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './admin-side-nav.html',
  styleUrl: './admin-side-nav.scss',
})
export class AdminSideNav {
  public readonly adminItems = input.required<AdminNavItem[]>();
}
