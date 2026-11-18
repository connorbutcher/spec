import { inject, Injectable, resource, Signal } from '@angular/core';
import { SpecSheetHelpers } from '../../../core/helpers/spec-sheet-helpers';
import { SpecSheetType } from '../../../core/models/domain/types/spec-sheet-type';
import { SheetListingService } from '../../../core/services/sheet-listing.service';

@Injectable()
export class SheetsDataService {
  private readonly _sheetListingService = inject(SheetListingService);

  public getSpecSheetsForPhaseAndTypeResource(
    phaseId: Signal<number | null>,
    specSheetType: Signal<SpecSheetType | null>,
  ) {
    return resource({
      params: () => {
        const type = specSheetType();
        const phase = phaseId();
        return phase && type ? { phaseId: phase, type } : undefined;
      },
      loader: async ({ params }) => {
        const summaries = await this._sheetListingService.getByPhaseAndTypeAsync(
          params.phaseId,
          params.type.id,
        );
        return SpecSheetHelpers.sortSummaries(summaries, params.type);
      },
    });
  }
}
