import { Dialog } from '@angular/cdk/dialog';
import { Component, Injector, model, Signal, viewChild } from '@angular/core';
import { InputText } from 'primeng/inputtext';
import { Table, TableModule, TableRowSelectEvent } from 'primeng/table';
import { firstValueFrom } from 'rxjs';
import { User } from '../../../../core/models/domain/access/user';
import { AdminService } from '../../admin.service';
import { PermissionsTableDisplay } from '../../components/permissions-table-display/permissions-table-display';
import { ManagePermissionsDialog } from '../permissions/dialogs/manage-permissions-dialog/manage-permissions-dialog';
import { ManagePermissionsDialogData } from '../permissions/dialogs/manage-permissions-dialog/models/manage-permissions-dialog-data.interface';

@Component({
  selector: 'app-users',
  imports: [InputText, TableModule, PermissionsTableDisplay],
  templateUrl: './users.html',
  styleUrl: './users.scss',
})
export class Users {
  public readonly usersTable = viewChild<Table>('usersTable');
  public readonly selectedUser = model<User>();

  constructor(
    private readonly _adminService: AdminService,
    private readonly _dialog: Dialog,
    private readonly _injector: Injector,
  ) {}

  public filter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.usersTable()?.filterGlobal(filterValue, 'contains');
  }

  public async onRowSelect(event: TableRowSelectEvent) {
    if (!event.data) {
      return;
    }

    const dialogRef = this._dialog.open<
      boolean,
      ManagePermissionsDialogData,
      ManagePermissionsDialog
    >(ManagePermissionsDialog, {
      data: {
        user: event.data,
        permissionsToManage: this._adminService.permissions(),
      },
      width: '30vw',
      injector: this._injector,
    });

    await firstValueFrom(dialogRef.closed);
    this.selectedUser.set(undefined);
  }

  public get users(): Signal<User[]> {
    return this._adminService.users;
  }
}
