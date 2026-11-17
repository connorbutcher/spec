import { CellValue } from './cell-value';

export type TextValue = CellValue & {
  value: string | null;
};
