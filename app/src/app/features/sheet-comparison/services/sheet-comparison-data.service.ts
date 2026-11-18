import { inject, Injectable } from '@angular/core';
import { SheetDetail } from '../../../core/models/domain/sheets/sheet-detail';
import { SheetListItem } from '../../../core/models/domain/sheets/sheet-list-item';
import { SheetListingService } from '../../../core/services/sheet-listing.service';
import { SheetDataService } from '../../sheet/services/sheet-data.service';

@Injectable()
export class SheetComparisonDataService {
  private readonly _sheetDataService = inject(SheetDataService);
  private readonly _sheetListingService = inject(SheetListingService);

  public async getSheetAsync(item: SheetListItem): Promise<SheetDetail> {
    return await this._sheetDataService.getSheetAsync(item);
  }

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
