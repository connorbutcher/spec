import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { CreateSpecSheetRequest } from '../../../../core/models/requests/spec-sheets/create-spec-sheet-request';

@Injectable()
export class CreatePUSpecSheetDialogDataService {
  private readonly _httpClient = inject(HttpClient);

  public async createSpecSheetAsync(
    request: CreateSpecSheetRequest,
  ): Promise<number> {
    const url = `${environment.apiUrl}/specsheets/create`;
    return await lastValueFrom(this._httpClient.post<number>(url, request));
  }
}
