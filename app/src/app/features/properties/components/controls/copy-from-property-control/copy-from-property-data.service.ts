import { inject, Injectable } from '@angular/core';
import { SheetListingService } from '../../../../../core/services/sheet-listing.service';
import { SheetListItem } from '../../../../../core/models/domain/sheets/sheet';

@Injectable()
export class CopyFromPropertyDataService {
  private readonly _sheetListingService = inject(SheetListingService);

  public async getAllSpecSheetsByTypeAsync(
    typeId: number,
  ): Promise<SheetListItem[]> {
    return await this._sheetListingService.getPublishedByTypeAsync(typeId);
  }
}
