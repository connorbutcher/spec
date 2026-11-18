import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import { SheetAdapter } from '../../../core/helpers/sheet-adapter';
import { Sheet } from '../../../core/models/domain/sheets/sheet';
import { SpecSheet } from '../../../core/models/domain/spec-sheet/spec-sheet';
import { SpecSheetSummary } from '../../../core/models/domain/spec-sheet/spec-sheet-summary';
import { SheetListingService } from '../../../core/services/sheet-listing.service';

@Injectable()
export class SheetComparisonDataService {
  private readonly _httpClient = inject(HttpClient);
  private readonly _sheetListingService = inject(SheetListingService);

  public async getSpecSheetByIdAsync(id: number): Promise<SpecSheet> {
    const url = `${environment.apiUrl}/sheets/spec-sheet/${id}/sheet`;
    const sheet = await lastValueFrom(this._httpClient.get<Sheet>(url));
    return SheetAdapter.toSpecSheet(sheet, id);
  }

  public async getAllSpecSheetsByPhaseAndTypeAsync(
    phaseId: number,
    typeId: number,
  ): Promise<SpecSheetSummary[]> {
    return await this._sheetListingService.getByPhaseAndTypeAsync(phaseId, typeId);
  }
}
