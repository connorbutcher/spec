import { httpResource } from '@angular/common/http';
import { Injectable, Signal } from '@angular/core';
import { environment } from '../../../../../environments/environment';
import { SpecSheetHelpers } from '../../../core/helpers/spec-sheet-helpers';
import { SpecSheetSummary } from '../../../core/models/domain/spec-sheet/spec-sheet-summary';
import { SpecSheetType } from '../../../core/models/domain/types/spec-sheet-type';

@Injectable()
export class SheetsDataService {
  public getSpecSheetsForPhaseAndTypeResource(
    phaseId: Signal<number | null>,
    specSheetType: Signal<SpecSheetType | null>,
  ) {
    return httpResource<SpecSheetSummary[]>(
      () => {
        const type = specSheetType();
        return phaseId() && type
          ? {
              url: `${environment.apiUrl}/specsheets/phase/${phaseId()}/type/${type.id}`,
            }
          : undefined;
      },
      {
        parse: (value) => {
          const type = specSheetType();
          return type
            ? SpecSheetHelpers.sortSummaries(value as SpecSheetSummary[], type)
            : [];
        },
      },
    );
  }
}
