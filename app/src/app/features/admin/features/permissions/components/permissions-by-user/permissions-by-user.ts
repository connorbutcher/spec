import { Component, computed, effect, input } from '@angular/core';
import { AdminService } from '../../../../admin.service';
import { PermissionsService } from '../../services/permissions.service';
import { UserPermissions } from './components/user-permissions/user-permissions';

@Component({
  selector: 'app-permissions-by-user',
  imports: [UserPermissions],
  templateUrl: './permissions-by-user.html',
  styleUrl: './permissions-by-user.scss',
})
export class PermissionsByUser {
  public readonly id = input.required<number>();
  public readonly selectedUser = computed(() => {
    const id = +this.id();
    if (!id) {
      return;
    }

    return this._adminService.users().find((user) => user.id === id);
  });

  constructor(
    private readonly _adminService: AdminService,
    private readonly _permissionsService: PermissionsService,
  ) {
    this._permissionsService.activeMode.set('byuser');
    effect(() => {
      this._permissionsService.activeId.set(+this.id());
    });
  }
}
