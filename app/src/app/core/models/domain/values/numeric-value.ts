import { CellValue } from './cell-value';

export type NumericValue = CellValue & {
  value: number | null;
};
