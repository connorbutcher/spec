import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { Component, Inject, signal } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ButtonDirective } from 'primeng/button';
import { Select } from 'primeng/select';
import { Permission } from '../../../../../../core/models/domain/access/permission';
import { User } from '../../../../../../core/models/domain/access/user';
import { UpdateUserPermissionsRequest } from '../../../../../../core/models/requests/access/update-user-permissions-request';
import { DialogHeader } from '../../../../../common/dialog-header/dialog-header';
import { ElementLabel } from '../../../../../common/element-label/element-label';
import { AdminService } from '../../../../admin.service';
import { ManagePermissionsDialogDataService } from './manage-permissions-dialog-data.service';
import { ManagePermissionsDialogData } from './models/manage-permissions-dialog-data.interface';

@Component({
  selector: 'app-manage-permissions-dialog',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    DialogHeader,
    ButtonDirective,
    ElementLabel,
    Select,
  ],
  templateUrl: './manage-permissions-dialog.html',
  styleUrl: './manage-permissions-dialog.scss',
  host: { class: 'dialog card' },
  providers: [ManagePermissionsDialogDataService],
})
export class ManagePermissionsDialog {
  public readonly userPermissionsCtrl = new FormControl<Permission[]>([]);
  public readonly isSaving = signal<boolean>(false);

  private readonly _relevantPermissionsForUser: Permission[];

  constructor(
    @Inject(DIALOG_DATA)
    private readonly _dialogData: ManagePermissionsDialogData,
    private readonly _dialogRef: DialogRef,
    private readonly _managePermissionsDialogDataService: ManagePermissionsDialogDataService,
    private readonly _adminService: AdminService,
    private readonly _messageService: MessageService,
  ) {
    this._dialogRef.disableClose = true;

    // Set the permissions to manage
    this._relevantPermissionsForUser = this._dialogData.user.permissions
      .filter((p) =>
        this._dialogData.permissionsToManage.some(
          (permission) => permission.id === p.permissionId,
        ),
      )
      .map((p) => p.permission);
    this.userPermissionsCtrl.setValue(this._relevantPermissionsForUser);
  }

  public async saveChangesAsync(): Promise<void> {
    if (this.userPermissionsCtrl.invalid || !this.userPermissionsCtrl.dirty) {
      return;
    }

    this.isSaving.set(true);

    const permissionsSelected = this.userPermissionsCtrl.value ?? [];

    // Get all new permissions
    const permissionIdsToAdd = permissionsSelected
      .filter(
        (permission) =>
          !this._relevantPermissionsForUser.some(
            (userPermission) => userPermission.id === permission.id,
          ),
      )
      .map((permission) => permission.id);

    // Get all removed permissions
    const permissionIdsToRemove = this._relevantPermissionsForUser
      .filter(
        (userPermission) =>
          !permissionsSelected.some(
            (permission) => permission.id === userPermission.id,
          ),
      )
      .map((userPermission) => userPermission.id);

    try {
      const request = {
        userId: this.user.id,
        permissionIdsToAdd,
        permissionIdsToRemove,
      } as UpdateUserPermissionsRequest;

      await this._managePermissionsDialogDataService.updateUserPermissionsAsync(
        request,
      );
      this._messageService.add({
        severity: 'success',
        summary: 'Permissions updated successfully.',
      });
      this._adminService.usersResource.reload();
      this._dialogRef.close();
    } catch (err) {
      this._messageService.add({
        severity: 'error',
        summary: 'Error updating permissions.',
      });
    } finally {
      this.isSaving.set(false);
    }
  }

  public close(): void {
    this._dialogRef.close();
  }

  public get user(): User {
    return this._dialogData.user;
  }

  public get permissions(): Permission[] {
    return this._dialogData.permissionsToManage.sort((a, b) =>
      a.title.localeCompare(b.title),
    );
  }
}
