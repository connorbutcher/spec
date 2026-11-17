import { CellValue } from './cell-value';

export type PairValue = CellValue & {
  valueX?: string;
  valueY?: string;
};
