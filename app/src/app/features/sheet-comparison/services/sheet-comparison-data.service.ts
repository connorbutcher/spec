import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { SpecSheet } from '../../../core/models/domain/spec-sheet/spec-sheet';
import { SpecSheetSummary } from '../../../core/models/domain/spec-sheet/spec-sheet-summary';

@Injectable()
export class SheetComparisonDataService {
  private readonly _httpClient = inject(HttpClient);

  public async getSpecSheetByIdAsync(
    id: number,
    includeDeleted: boolean = true,
  ): Promise<SpecSheet> {
    let queryParams = new HttpParams().set(
      'includeDeleted',
      includeDeleted.toString(),
    );

    const url = `${environment.apiUrl}/specsheets/${id}`;
    return await lastValueFrom(
      this._httpClient.get<SpecSheet>(url, { params: queryParams }),
    );
  }

  public async getAllSpecSheetsByPhaseAndTypeAsync(
    phaseId: number,
    typeId: number,
  ): Promise<SpecSheetSummary[]> {
    const url = `${environment.apiUrl}/specsheets/phase/${phaseId}/type/${typeId}`;
    return await lastValueFrom(this._httpClient.get<SpecSheetSummary[]>(url));
  }
}
