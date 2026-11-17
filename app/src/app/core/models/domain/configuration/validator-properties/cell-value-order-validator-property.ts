import { ValidatorProperty } from './validator-property';

export type CellValueOrderValidatorProperty = ValidatorProperty & {
  cellKeys: string[];
  order: 'asc' | 'desc';
};
