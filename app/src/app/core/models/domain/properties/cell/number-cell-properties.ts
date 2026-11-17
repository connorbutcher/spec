import { CellProperties } from './cell-properties';

export type NumberCellProperties = CellProperties & {
  decimalPlaces: number;
  defaultValue?: number;
};
