import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { Component, computed, Inject, signal } from '@angular/core';
import {
  FormControl,
  FormGroup,
  FormsModule,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ButtonDirective } from 'primeng/button';
import { Select } from 'primeng/select';
import { Permission } from '../../../../../../core/models/domain/access/permission';
import { User } from '../../../../../../core/models/domain/access/user';
import { UpdateUserPermissionsRequest } from '../../../../../../core/models/requests/access/update-user-permissions-request';
import { DialogHeader } from '../../../../../common/dialog-header/dialog-header';
import { ElementLabel } from '../../../../../common/element-label/element-label';
import { AdminService } from '../../../../admin.service';
import { ManagePermissionsDialogDataService } from '../manage-permissions-dialog/manage-permissions-dialog-data.service';
import { ManagePermissionsForSpecSheetTypeDialogData } from './models/manage-permissions-for-spec-sheet-type-dialog-data';

interface ManagePermissionsForPUCardTypeForm {
  user: FormControl<User>;
  permissions: FormControl<Permission[]>;
}

@Component({
  selector: 'app-manage-permissions-for-spec-sheet-type-dialog',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    Select,
    ButtonDirective,
    ElementLabel,
    DialogHeader,
  ],
  templateUrl: './manage-permissions-for-spec-sheet-type-dialog.html',
  styleUrl: './manage-permissions-for-spec-sheet-type-dialog.scss',
  host: { class: 'dialog card' },
  providers: [ManagePermissionsDialogDataService],
})
export class ManagePermissionsForSpecSheetTypeDialog {
  public readonly form: FormGroup<ManagePermissionsForPUCardTypeForm>;
  public readonly isSaving = signal<boolean>(false);
  public readonly selectableUsers = computed(() => {
    const users = this._adminService.users();
    const specSheetType = this._dialogData.specSheetType;

    return users.filter((user) =>
      specSheetType.permissions.every((permission) =>
        user.permissions.every(
          (userPermission) =>
            userPermission.permissionId !== permission.permissionId,
        ),
      ),
    );
  });

  constructor(
    @Inject(DIALOG_DATA)
    private readonly _dialogData: ManagePermissionsForSpecSheetTypeDialogData,
    private readonly _dialogRef: DialogRef,
    private readonly _managePermissionsDialogDataService: ManagePermissionsDialogDataService,
    private readonly _adminService: AdminService,
    private readonly _messageService: MessageService,
    private readonly _formBuilder: NonNullableFormBuilder,
  ) {
    this._dialogRef.disableClose = true;

    this.form = this.buildForm();
  }

  public async savePermissionsAsync(): Promise<void> {
    if (this.form.invalid || this.isSaving()) {
      return;
    }

    this.isSaving.set(true);

    try {
      const request = {
        userId: this.form.value.user?.id,
        permissionIdsToRemove: [],
        permissionIdsToAdd: this.form.value.permissions?.map(
          (permission) => permission.id,
        ),
      } as UpdateUserPermissionsRequest;

      await this._managePermissionsDialogDataService.updateUserPermissionsAsync(
        request,
      );
      this._messageService.add({
        severity: 'success',
        summary: `Permissions updated successfully`,
      });
      this._dialogRef.close(true);
    } catch (err) {
      this._messageService.add({
        severity: 'error',
        summary: `Error updating permissions`,
        detail: (err as Error).message,
      });
    } finally {
      this.isSaving.set(false);
    }
  }

  public close(): void {
    this._dialogRef.close();
  }

  public hasError(
    controlName: keyof ManagePermissionsForPUCardTypeForm,
    errorName: string,
  ): boolean {
    const control = this.form.get(controlName);
    return control! && control.hasError(errorName) && control.dirty;
  }

  public get permissions(): Permission[] {
    return this._dialogData.specSheetType.permissions
      .map((p) => p.permission!)
      .sort((a, b) => a.title.localeCompare(b.title));
  }

  private buildForm(): FormGroup<ManagePermissionsForPUCardTypeForm> {
    return this._formBuilder.group({
      user: this._formBuilder.control<User>(undefined!, [Validators.required]),
      permissions: this._formBuilder.control<Permission[]>(undefined!, [
        Validators.required,
        Validators.minLength(1),
      ]),
    });
  }
}
