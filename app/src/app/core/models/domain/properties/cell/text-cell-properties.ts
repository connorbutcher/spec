import { CellProperties } from './cell-properties';

export type TextCellProperties = CellProperties & {
  defaultValue?: string;
};
