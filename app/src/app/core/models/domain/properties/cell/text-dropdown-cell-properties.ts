import { CellProperties } from './cell-properties';

export type TextDropdownCellProperties = CellProperties & {
  options: string[];
  defaultValue?: string;
};
