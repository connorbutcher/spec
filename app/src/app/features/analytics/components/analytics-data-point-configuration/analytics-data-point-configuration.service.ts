import { httpResource, HttpResourceRef } from '@angular/common/http';
import { Signal } from '@angular/core';
import { environment } from '../../../../../../environments/environment';

export class AnalyticsDataPointConfigurationService {
  public getPhasesAllCellsAreInUseResource(
    puCardTypeId: Signal<number | undefined>,
    referenceIds: Signal<string[] | undefined>,
  ): HttpResourceRef<number[] | undefined> {
    return httpResource<number[]>(
      () => {
        const puCardTypeIdValue = puCardTypeId();
        const referenceIdsValue = referenceIds();
        if (puCardTypeIdValue == null || !referenceIdsValue?.length) {
          return undefined;
        }

        return {
          url: `${environment.apiUrl}/templates/${puCardTypeIdValue}/cells/inuse/phases`,
          method: 'POST',
          body: referenceIdsValue,
        };
      },
      { defaultValue: [] },
    );
  }
}
