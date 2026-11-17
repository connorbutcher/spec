import { Dialog, DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import {
  CdkDragDrop,
  DragDropModule,
  moveItemInArray,
  transferArrayItem,
} from '@angular/cdk/drag-drop';
import {
  Component,
  computed,
  effect,
  inject,
  linkedSignal,
  model,
  resource,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ButtonDirective } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { firstValueFrom } from 'rxjs';
import { User } from '../../../../core/models/domain/access/user';
import { CreateTablePermissionRequest } from '../../../../core/models/requests/access/tables/create-table-permission-request';
import { UserService } from '../../../../core/services/user.service';
import { ConfirmationDialog } from '../../../common/confirmation-dialog/confirmation-dialog';
import { ConfirmationDialogData } from '../../../common/confirmation-dialog/models/confirmation-dialog-data.interface';
import { DialogHeader } from '../../../common/dialog-header/dialog-header';
import { ConfigureTableAccessDialogDataService } from './configure-table-access-dialog-data.service';
import { ConfigureTableAccessDialogData } from './models/configure-table-access-dialog-data';

@Component({
  selector: 'app-configure-table-access-dialog',
  imports: [
    FormsModule,
    DialogHeader,
    Message,
    ButtonDirective,
    DragDropModule,
    InputText,
  ],
  templateUrl: './configure-table-access-dialog.html',
  styleUrl: './configure-table-access-dialog.scss',
  host: { class: 'dialog card' },
  providers: [ConfigureTableAccessDialogDataService],
})
export class ConfigureTableAccessDialog {
  public readonly usersFilter = model<string>('');
  public readonly users = linkedSignal(() => this.allUsers());
  public readonly allUsers = computed(() => this._usersResource.value() ?? []);
  public readonly tablePermissions = computed(() =>
    this._tablePermissionResource.value(),
  );
  public readonly canSaveChanges = signal(false);

  public readonly admins = signal<User[]>([]);
  public readonly access = signal<User[]>([]);
  public readonly isInRestrictAccessMode = signal<boolean>(false);
  public readonly isSaving = signal<boolean>(false);

  private readonly _tablePermissionResource = resource({
    params: () => ({ referenceId: this._dialogData.table.referenceId() }),
    loader: ({ params }) =>
      this._configureTableAccessDialogDataService.getTablePermissionByReferenceIdAsync(
        params.referenceId,
      ),
  });
  private readonly _usersResource = resource({
    loader: () =>
      this._configureTableAccessDialogDataService.getAllUsersAsync(),
  });

  private readonly _configureTableAccessDialogDataService = inject(
    ConfigureTableAccessDialogDataService,
  );
  private readonly _userService = inject(UserService);
  private readonly _messageService = inject(MessageService);
  private readonly _dialog = inject(Dialog);
  private readonly _dialogRef = inject(
    DialogRef<boolean, ConfigureTableAccessDialog>,
  );
  private readonly _dialogData =
    inject<ConfigureTableAccessDialogData>(DIALOG_DATA);

  constructor() {
    effect(() => {
      const rawUsersFilter = this.usersFilter();
      const allUsers = this.allUsers();
      const admins = this.admins();
      const access = this.access();
      const excludedUserIds = new Set<number>([
        ...admins.map((u) => u.id),
        ...access.map((u) => u.id),
      ]);
      const filteredAllUsers = allUsers.filter(
        (u) => !excludedUserIds.has(u.id),
      );
      if (!rawUsersFilter) {
        this.users.set(filteredAllUsers);
        return;
      }

      const usersFilter = rawUsersFilter.toLocaleLowerCase();
      const filteredUsers = filteredAllUsers.filter((u) =>
        u.displayName.toLocaleLowerCase().includes(usersFilter),
      );
      this.users.set(filteredUsers);
    });

    effect(() => {
      const tablePermission = this.tablePermissions();
      if (!tablePermission) {
        this.admins.set([]);
        this.access.set([]);
        return;
      }

      const admins = this.allUsers().filter((u) =>
        tablePermission.some((tp) => tp.userId === u.id && tp.isAdmin),
      );
      const access = this.allUsers().filter((u) =>
        tablePermission.some((tp) => tp.userId === u.id && !tp.isAdmin),
      );
      this.admins.set(admins);
      this.access.set(access);
    });
  }

  public async savePermissionChangesAsync(): Promise<void> {
    if (this.isSaving()) {
      return;
    }

    this.isSaving.set(true);

    try {
      const admins = this.admins();
      const access = this.access();
      const tableReferenceId = this._dialogData.table.referenceId();
      const existingPermissions = this.tablePermissions() ?? [];

      const createTablePermissionRequests: CreateTablePermissionRequest[] = [];
      const deleteTablePermissionRequests: number[] = [];

      // Determine permissions to create
      for (const admin of admins) {
        if (
          !existingPermissions.some(
            (tp) => tp.userId === admin.id && tp.isAdmin,
          )
        ) {
          createTablePermissionRequests.push({
            tableReferenceId,
            userId: admin.id,
            isAdmin: true,
          });
        }
      }

      for (const user of access) {
        if (
          !existingPermissions.some(
            (tp) => tp.userId === user.id && !tp.isAdmin,
          )
        ) {
          createTablePermissionRequests.push({
            tableReferenceId,
            userId: user.id,
            isAdmin: false,
          });
        }
      }

      // Determine permissions to delete
      for (const permission of existingPermissions) {
        const isAdminPermission = admins.some(
          (u) => u.id === permission.userId,
        );
        const isAccessPermission = access.some(
          (u) => u.id === permission.userId,
        );
        if (!isAdminPermission && !isAccessPermission) {
          deleteTablePermissionRequests.push(permission.id);
        }

        // If the permission type has changed (admin <-> access), we need to delete the old one
        if (
          (permission.isAdmin && !isAdminPermission) ||
          (!permission.isAdmin && isAdminPermission)
        ) {
          deleteTablePermissionRequests.push(permission.id);
        }
      }

      const request = {
        tableReferenceId,
        createTablePermissionRequests,
        deleteTablePermissionRequests,
      };
      await this._configureTableAccessDialogDataService.updateTablePermissionsAsync(
        request,
      );
      this._messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: 'Table permissions have been updated successfully.',
      });
      this._dialogRef.close(true);
    } catch (error) {
      this._messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'An error occurred while saving table permissions.',
      });
    } finally {
      this.isSaving.set(false);
    }
  }

  public async removePermissionsAsync(): Promise<void> {
    if (this.isSaving()) {
      return;
    }

    const confirmationRef = this._dialog.open<
      boolean,
      ConfirmationDialogData,
      ConfirmationDialog
    >(ConfirmationDialog, {
      data: {
        title: 'Confirm Remove All Permissions',
        message:
          'Are you sure you want to remove all permissions for this table? This will mean all users will be able to view and edit this table.',
        confirmSeverity: 'danger',
      },
    });

    const confirmResult = await firstValueFrom(confirmationRef.closed);
    if (!confirmResult) {
      return;
    }

    this.isSaving.set(true);

    try {
      await this._configureTableAccessDialogDataService.deleteTablePermissionsByReferenceIdAsync(
        this._dialogData.table.referenceId(),
      );
      this._messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: 'All table permissions have been removed.',
      });
      this._dialogRef.close(true);
    } catch (error) {
      this._messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'An error occurred while removing table permissions.',
      });
    } finally {
      this.isSaving.set(false);
    }
  }

  public drop(event: CdkDragDrop<User[]>): void {
    if (event.previousContainer === event.container) {
      moveItemInArray(
        event.container.data,
        event.previousIndex,
        event.currentIndex,
      );
    } else {
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex,
      );
    }

    this.admins.update((admins) =>
      admins.sort((a, b) => a.displayName.localeCompare(b.displayName)),
    );
    this.access.update((access) =>
      access.sort((a, b) => a.displayName.localeCompare(b.displayName)),
    );

    this.setCanSaveChanges();
  }

  public toggleRestrictAccess(): void {
    this.isInRestrictAccessMode.set(!this.isInRestrictAccessMode());

    const currentUser = this.allUsers().find(
      (u) => u.id === this._userService.user()!.id,
    );
    if (!currentUser) {
      return;
    }

    this.admins.set([currentUser]);
    this.setCanSaveChanges();
  }

  public close(): void {
    this._dialogRef.close();
  }

  public get title(): string {
    return this._dialogData.table.title().toLocaleUpperCase();
  }

  private setCanSaveChanges(): void {
    const admins = this.admins();
    const access = this.access();
    const tablePermissions = this.tablePermissions();
    if (!admins.length) {
      this.canSaveChanges.set(false);
      return;
    }

    if (!tablePermissions) {
      this.canSaveChanges.set(true); // No existing permissions, so any change is new
      return;
    }

    const combinedUsers = [...admins, ...access];
    if (combinedUsers.length !== tablePermissions.length) {
      this.canSaveChanges.set(true); // Different number of users means a change
      return;
    }

    for (const admin of admins) {
      const permission = tablePermissions.find((tp) => tp.userId === admin.id);
      if (!permission || !permission.isAdmin) {
        this.canSaveChanges.set(true); // Admin user not found or not admin in existing permissions
        return;
      }
    }

    for (const user of access) {
      const permission = tablePermissions.find((tp) => tp.userId === user.id);
      if (!permission || permission.isAdmin) {
        this.canSaveChanges.set(true); // Access user not found or is admin in existing permissions
        return;
      }
    }

    this.canSaveChanges.set(false); // No changes detected
  }
}
