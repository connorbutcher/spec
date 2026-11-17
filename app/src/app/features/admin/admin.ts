import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ConditionalAccessService } from '../../core/services/conditional-access.service';
import { AdminService } from './admin.service';
import { AdminSideNav } from './components/admin-side-nav/admin-side-nav';
import { AdminNavItem } from './models/admin-nav-item.class';

@Component({
  selector: 'app-admin',
  imports: [AdminSideNav, RouterOutlet],
  templateUrl: './admin.html',
  styleUrl: './admin.scss',
  providers: [AdminService],
})
export class Admin {
  public readonly adminItems: AdminNavItem[] = [];

  constructor(
    private readonly _adminService: AdminService,
    private readonly _conditionalAccessService: ConditionalAccessService,
  ) {
    if (this._conditionalAccessService.has('admin.sheets')) {
      this.adminItems.push(new AdminNavItem('Sheets', 'build', 'sheets'));
    }

    if (this._conditionalAccessService.has('admin.types')) {
      this.adminItems.push(new AdminNavItem('Types', 'view_list', 'types'));
    }

    if (this._conditionalAccessService.has('admin.phases')) {
      this.adminItems.push(new AdminNavItem('Phases', 'timeline', 'phases'));
    }

    if (this._conditionalAccessService.has('admin.users')) {
      this.adminItems.push(new AdminNavItem('Users', 'person', 'users'));
    }

    if (this._conditionalAccessService.has('admin.permissions')) {
      this.adminItems.push(
        new AdminNavItem('Permissions', 'folder_supervised', 'permissions'),
      );
    }
  }
}
