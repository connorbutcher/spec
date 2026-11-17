import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { environment } from '../../../../../../../../environments/environment';
import { UpdateUserPermissionsRequest } from '../../../../../../core/models/requests/access/update-user-permissions-request';

@Injectable()
export class ManagePermissionsDialogDataService {
  private readonly _httpClient = inject(HttpClient);

  public async updateUserPermissionsAsync(
    request: UpdateUserPermissionsRequest,
  ): Promise<void> {
    const url = `${environment.apiUrl}/users/permissions/update`;
    return await lastValueFrom(this._httpClient.patch<void>(url, request));
  }
}
