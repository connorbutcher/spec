import { ValidatorLevel } from '../../../core/types/configuration/validator-level.enum';

export type SpecSheetAdvisory = {
  level: ValidatorLevel;
  message: string;
  referenceId: string;
};
