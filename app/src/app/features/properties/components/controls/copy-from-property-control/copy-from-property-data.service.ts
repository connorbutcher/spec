import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { environment } from '../../../../../../../environments/environment';
import { SpecSheetSummary } from '../../../../../core/models/domain/spec-sheet/spec-sheet-summary';

@Injectable()
export class CopyFromPropertyDataService {
  constructor(private readonly _httpClient: HttpClient) {}

  public async getAllSpecSheetsByTypeAsync(
    typeId: number,
  ): Promise<SpecSheetSummary[]> {
    const url = `${environment.apiUrl}/specsheets/type/${typeId}`;
    return await lastValueFrom(this._httpClient.get<SpecSheetSummary[]>(url));
  }
}
