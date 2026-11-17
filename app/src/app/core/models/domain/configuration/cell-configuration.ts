import { CellProperties } from '../properties/cell/cell-properties';
import { CellStyling } from '../styling/cell-styling';
import { Validator } from './validator';

export type CellConfiguration = {
  id: number;
  styling: CellStyling;
  properties: CellProperties;
  validators?: Validator[];
  isIdentifier: boolean;
};
