import { Dialog } from '@angular/cdk/dialog';
import {
  Component,
  computed,
  effect,
  Injector,
  input,
  model,
  viewChild,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonDirective } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Table, TableModule, TableRowSelectEvent } from 'primeng/table';
import { firstValueFrom } from 'rxjs';
import { User } from '../../../../../../core/models/domain/access/user';
import { SpecSheetType } from '../../../../../../core/models/domain/types/spec-sheet-type';
import { TypesService } from '../../../../../../core/services/types.service';
import { AdminService } from '../../../../admin.service';
import { ManagePermissionsDialog } from '../../dialogs/manage-permissions-dialog/manage-permissions-dialog';
import { ManagePermissionsDialogData } from '../../dialogs/manage-permissions-dialog/models/manage-permissions-dialog-data.interface';
import { ManagePermissionsForSpecSheetTypeDialog } from '../../dialogs/manage-permissions-for-spec-sheet-type-dialog/manage-permissions-for-spec-sheet-type-dialog';
import { ManagePermissionsForSpecSheetTypeDialogData } from '../../dialogs/manage-permissions-for-spec-sheet-type-dialog/models/manage-permissions-for-spec-sheet-type-dialog-data';
import { PermissionsService } from '../../services/permissions.service';
import { PermissionsTableDisplay } from './components/permissions-table-display/permissions-table-display';

@Component({
  selector: 'app-permissions-by-spec-sheet-type',
  imports: [
    FormsModule,
    InputText,
    ButtonDirective,
    TableModule,
    PermissionsTableDisplay,
  ],
  templateUrl: './permissions-by-spec-sheet-type.html',
  styleUrl: './permissions-by-spec-sheet-type.scss',
})
export class PermissionsBySpecSheetType {
  public readonly id = input.required<number>();
  public readonly usersTable = viewChild<Table>('usersTable');
  public readonly selectedUser = model<User>();
  public readonly selectedSpecSheetType = computed<SpecSheetType | undefined>(
    () => {
      const id = +this.id();
      if (!id) {
        return;
      }
      const specSheetTypes = this._typesService.specSheetTypes();
      if (!specSheetTypes) {
        return undefined;
      }

      return specSheetTypes.find((sst) => sst.id === id);
    },
  );
  public readonly usersWithPermissionsForType = computed(() => {
    const users = this._adminService.users();
    if (!users) {
      return [];
    }
    const specSheetType = this.selectedSpecSheetType();
    if (!specSheetType) {
      return [];
    }
    const permissionsForType = specSheetType.permissions;
    const usersWithPermissions: User[] = [];
    for (const user of users) {
      if (
        permissionsForType.some((pft) =>
          user.permissions.some((p) => p.permissionId === pft.permissionId),
        )
      ) {
        usersWithPermissions.push(user);
      }
    }
    return usersWithPermissions;
  });

  constructor(
    private readonly _typesService: TypesService,
    private readonly _permissionsService: PermissionsService,
    private readonly _adminService: AdminService,
    private readonly _dialog: Dialog,
    private readonly _injector: Injector,
  ) {
    this._permissionsService.activeMode.set('bytype');
    effect(() => {
      this._permissionsService.activeId.set(+this.id());
    });
  }

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
        permissionsToManage: this.selectedSpecSheetType()!.permissions.map(
          (p) => p.permission!,
        ),
      },
      width: '30vw',
      injector: this._injector,
    });
    await firstValueFrom(dialogRef.closed);
    this.selectedUser.set(undefined);
  }

  public addUserPermissions(): void {
    this._dialog.open<
      unknown,
      ManagePermissionsForSpecSheetTypeDialogData,
      ManagePermissionsForSpecSheetTypeDialog
    >(ManagePermissionsForSpecSheetTypeDialog, {
      data: { specSheetType: this.selectedSpecSheetType()! },
      width: '30vw',
      injector: this._injector,
    });
  }
}
