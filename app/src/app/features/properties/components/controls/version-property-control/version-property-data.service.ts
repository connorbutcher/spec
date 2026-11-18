import { inject, Injectable } from '@angular/core';
import { SpecSheetSummary } from '../../../../../core/models/domain/spec-sheet/spec-sheet-summary';
import { SheetListingService } from '../../../../../core/services/sheet-listing.service';

@Injectable()
export class VersionPropertyDataService {
  private readonly _sheetListingService = inject(SheetListingService);

  public async getAllSpecSheetsByPhaseAndTypeAsync(
    phaseId: number,
    typeId: number,
  ): Promise<SpecSheetSummary[]> {
    return await this._sheetListingService.getByPhaseAndTypeAsync(phaseId, typeId);
  }
}
