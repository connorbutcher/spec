import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SheetAdapter } from '../helpers/sheet-adapter';
import { SpecSheetSummary } from '../models/domain/spec-sheet/spec-sheet-summary';
import { SheetListing, SheetVersionSummary } from '../models/domain/sheets/sheet';

/**
 * Lists of spec sheets from the sheets endpoints: a published version of a sheet is a spec sheet, and so is its draft.
 */
@Injectable({ providedIn: 'root' })
export class SheetListingService {
  private readonly _httpClient = inject(HttpClient);

  /** The published versions of the sheet for the phase and type, then its draft when it has one. */
  public async getByPhaseAndTypeAsync(
    phaseId: number,
    typeId: number,
  ): Promise<SpecSheetSummary[]> {
    const url = `${environment.apiUrl}/sheets/phase/${phaseId}/type/${typeId}/listing`;
    const listing = await lastValueFrom(this._httpClient.get<SheetListing>(url));
    return SheetAdapter.toSummaries(listing.versions, listing.sheet ? [listing.sheet] : []);
  }

  /** The published versions of every sheet of the type. */
  public async getPublishedByTypeAsync(typeId: number): Promise<SpecSheetSummary[]> {
    const url = `${environment.apiUrl}/sheets/type/${typeId}/versions`;
    const versions = await lastValueFrom(this._httpClient.get<SheetVersionSummary[]>(url));
    return SheetAdapter.toSummaries(versions, []);
  }
}
