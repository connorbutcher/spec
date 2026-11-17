import { CellProperties } from './cell-properties';

export type PairCellProperties = CellProperties & {
  defaultValueX?: string;
  defaultValueY?: string;
};
