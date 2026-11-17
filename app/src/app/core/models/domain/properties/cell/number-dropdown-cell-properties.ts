import { CellProperties } from './cell-properties';

export type NumberDropdownCellProperties = CellProperties & {
  options: number[];
  defaultValue?: number;
};
