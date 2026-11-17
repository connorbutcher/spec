import { computed, Signal } from '@angular/core';
import { AdvisoryStateHelpers } from '../../../../core/helpers/advisory-state-helpers';
import { ValidatorLevel } from '../../../../core/types/configuration/validator-level.enum';
import { SpecSheetAdvisory } from '../spec-sheet-advisory';

export class AdvisoryAggregator {
  public static aggregate(
    nodeAdvisories: Signal<SpecSheetAdvisory[]>,
    childAdvisories: Signal<SpecSheetAdvisory[]>[],
  ): Signal<SpecSheetAdvisory[]> {
    return computed(() => {
      const allAdvisories: SpecSheetAdvisory[] = [...nodeAdvisories()];

      for (const childSignal of childAdvisories) {
        allAdvisories.push(...childSignal());
      }

      return allAdvisories;
    });
  }

  public static getWorstState(
    advisories: Signal<SpecSheetAdvisory[]>,
  ): Signal<ValidatorLevel | undefined> {
    return computed(() =>
      AdvisoryStateHelpers.getWorstAdvisoryState(advisories()),
    );
  }
}
