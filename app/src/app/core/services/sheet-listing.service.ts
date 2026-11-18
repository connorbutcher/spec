import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SheetListItem } from '../models/domain/sheets/sheet-list-item';

/**
 * Lists of sheets: the published versions of a sheet, then its draft when it has one.
 */
@Injectable({ providedIn: 'root' })
export class SheetListingService {
  private readonly _httpClient = inject(HttpClient);

  /** The published versions of the sheet for the phase and type, then its draft when it has one. */
  public async getByPhaseAndTypeAsync(
    phaseId: number,
    typeId: number,
  ): Promise<SheetListItem[]> {
    const url = `${environment.apiUrl}/sheets/phase/${phaseId}/type/${typeId}/list`;
    return await lastValueFrom(this._httpClient.get<SheetListItem[]>(url));
  }

  /** The published versions of every sheet of the type. */
  public async getPublishedByTypeAsync(
    typeId: number,
  ): Promise<SheetListItem[]> {
    const url = `${environment.apiUrl}/sheets/type/${typeId}/list`;
    return await lastValueFrom(this._httpClient.get<SheetListItem[]>(url));
  }
}
