import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { SheetLocation } from '../../../../core/models/domain/sheets/sheet';
import { CreateSheetRequest } from '../../../../core/models/requests/sheets/create-sheet-request';

@Injectable()
export class CreatePUSpecSheetDialogDataService {
  private readonly _httpClient = inject(HttpClient);

  /** Starts the draft, and returns the id of the sheet. */
  public async createSpecSheetAsync(
    request: CreateSheetRequest,
  ): Promise<number> {
    const url = `${environment.apiUrl}/sheets/draft`;
    const location = await lastValueFrom(
      this._httpClient.post<SheetLocation>(url, request),
    );
    return location.sheetId;
  }
}
