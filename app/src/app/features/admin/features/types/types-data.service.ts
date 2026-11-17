import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { CreateSpecSheetTypeRequest } from '../../../../core/models/requests/types/type/create-spec-sheet-type-request';
import { UpdateSpecSheetTypeRequest } from '../../../../core/models/requests/types/type/update-spec-sheet-type-request';

@Injectable()
export class TypesDataService {
  private readonly _httpClient = inject(HttpClient);

  public async createSpecSheetTypeAsync(
    request: CreateSpecSheetTypeRequest,
  ): Promise<void> {
    const url = `${environment.apiUrl}/specsheettypes/create`;
    return await lastValueFrom(this._httpClient.post<void>(url, request));
  }

  public async updateSpecSheetTypeAsync(
    request: UpdateSpecSheetTypeRequest,
  ): Promise<void> {
    const url = `${environment.apiUrl}/specsheettypes/update`;
    return await lastValueFrom(this._httpClient.patch<void>(url, request));
  }
}
