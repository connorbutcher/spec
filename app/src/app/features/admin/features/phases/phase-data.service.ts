import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { environment } from '../../../../../../environments/environment';
import { CreateSpecSheetPhaseRequest } from '../../../../core/models/requests/types/phase/create-spec-sheet-phase-request';
import { UpdateSpecSheetPhaseRequest } from '../../../../core/models/requests/types/phase/update-spec-sheet-phase-request';

@Injectable()
export class PhaseDataService {
  private readonly _httpClient = inject(HttpClient);

  public async createSpecSheetPhaseAsync(
    request: CreateSpecSheetPhaseRequest,
  ): Promise<void> {
    const url = `${environment.apiUrl}/specsheetphases/create`;
    return await lastValueFrom(this._httpClient.post<void>(url, request));
  }

  public async updateSpecSheetPhaseAsync(
    request: UpdateSpecSheetPhaseRequest,
  ): Promise<void> {
    const url = `${environment.apiUrl}/specsheetphases/update`;
    return await lastValueFrom(this._httpClient.patch<void>(url, request));
  }

  public async updateSpecSheetPhasesAsync(
    requests: UpdateSpecSheetPhaseRequest[],
  ): Promise<void> {
    const url = `${environment.apiUrl}/specsheetphases/update/many`;
    return await lastValueFrom(this._httpClient.patch<void>(url, requests));
  }
}
