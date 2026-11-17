import { SpecSheetAdvisory } from '../../features/sheet/models/spec-sheet-advisory';
import { ValidatorLevel } from '../types/configuration/validator-level.enum';

export class AdvisoryStateHelpers {
  private static readonly _warningClass = 'invalid-row-warning';
  private static readonly _errorClass = 'invalid-row-error';

  public static getWorstAdvisoryState(
    advisories: SpecSheetAdvisory[],
  ): ValidatorLevel {
    const maxLevel = advisories.reduce((max, advisory) => {
      return advisory.level > max ? advisory.level : max;
    }, ValidatorLevel.Info);

    return maxLevel;
  }

  public static getAdvisoryStateClass(level: ValidatorLevel): string {
    switch (level) {
      case ValidatorLevel.Warning:
        return this._warningClass;
      case ValidatorLevel.Error:
        return this._errorClass;
      default:
        return '';
    }
  }
}
