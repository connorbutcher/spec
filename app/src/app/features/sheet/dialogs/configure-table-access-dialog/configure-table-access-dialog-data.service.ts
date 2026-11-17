import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { SpecSheetTablePermission } from '../../../../core/models/domain/access/spec-sheet-table-permission';
import { User } from '../../../../core/models/domain/access/user';
import { ManageTablePermissionsRequest } from '../../../../core/models/requests/access/tables/manage-table-permissions-request';

@Injectable()
export class ConfigureTableAccessDialogDataService {
  private readonly _httpClient = inject(HttpClient);

  public async getAllUsersAsync(): Promise<User[]> {
    const url = `${environment.apiUrl}/users/all`;
    return await lastValueFrom(this._httpClient.get<User[]>(url));
  }

  public async getTablePermissionByReferenceIdAsync(
    referenceId: string,
  ): Promise<SpecSheetTablePermission[]> {
    const url = `${environment.apiUrl}/permissions/tablepermissions/${referenceId}`;
    return await lastValueFrom(
      this._httpClient.get<SpecSheetTablePermission[]>(url),
    );
  }

  public async updateTablePermissionsAsync(
    request: ManageTablePermissionsRequest,
  ): Promise<void> {
    const url = `${environment.apiUrl}/permissions/tablepermissions/update`;
    return await lastValueFrom(this._httpClient.patch<void>(url, request));
  }

  public async deleteTablePermissionsByReferenceIdAsync(
    referenceId: string,
  ): Promise<void> {
    const url = `${environment.apiUrl}/permissions/tablepermissions/${referenceId}`;
    await lastValueFrom(this._httpClient.delete<void>(url));
  }
}
