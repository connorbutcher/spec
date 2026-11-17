import { ValidatorLevel } from '../../../types/configuration/validator-level.enum';
import { ValidatorProperty } from './validator-properties/validator-property';

export type ValidatorType =
  | 'required'
  | 'anyCellsRequired'
  | 'allCellsRequired'
  | 'cellValueOrder';

export type Validator = {
  type: ValidatorType;
  message: string;
  level: ValidatorLevel;
  property: ValidatorProperty;
};
