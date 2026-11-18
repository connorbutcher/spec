import { inject, Injectable } from '@angular/core';
import { SheetListingService } from '../../../../../core/services/sheet-listing.service';
import { SheetListItem } from '../../../../../core/models/domain/sheets/sheet-list-item';

@Injectable()
export class VersionPropertyDataService {
  private readonly _sheetListingService = inject(SheetListingService);

  public async getAllSpecSheetsByPhaseAndTypeAsync(
    phaseId: number,
    typeId: number,
  ): Promise<SheetListItem[]> {
    return await this._sheetListingService.getByPhaseAndTypeAsync(
      phaseId,
      typeId,
    );
  }
}
