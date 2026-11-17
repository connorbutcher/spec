import { Dialog } from '@angular/cdk/dialog';
import { Component, inject, Injector, input, viewChild } from '@angular/core';
import { ButtonDirective } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Table, TableModule } from 'primeng/table';
import { User } from '../../../../../../../../core/models/domain/access/user';
import { AdminService } from '../../../../../../admin.service';
import { ManagePermissionsDialog } from '../../../../dialogs/manage-permissions-dialog/manage-permissions-dialog';
import { ManagePermissionsDialogData } from '../../../../dialogs/manage-permissions-dialog/models/manage-permissions-dialog-data.interface';

@Component({
  selector: 'app-user-permissions',
  imports: [TableModule, InputText, ButtonDirective],
  templateUrl: './user-permissions.html',
  styleUrl: './user-permissions.scss',
})
export class UserPermissions {
  public readonly selectedUser = input.required<User>();
  public readonly permissionTable = viewChild<Table>('permissionTable');

  private readonly _adminService = inject(AdminService);
  private readonly _dialog = inject(Dialog);
  private readonly _injector = inject(Injector);

  public filter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.permissionTable()?.filterGlobal(filterValue, 'contains');
  }

  public managePermissions() {
    this._dialog.open<
      boolean,
      ManagePermissionsDialogData,
      ManagePermissionsDialog
    >(ManagePermissionsDialog, {
      data: {
        user: this.selectedUser()!,
        permissionsToManage: this._adminService.permissions(),
      },
      width: '30vw',
      injector: this._injector,
    });
  }
}
