import { ValidatorProperty } from './validator-property';

export type CellsRequiredValidatorProperty = ValidatorProperty & {
  cellKeys: string[];
};
