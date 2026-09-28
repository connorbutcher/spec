import { CellKind } from './cell-kind';

/** The body for creating or replacing a cell type. `options` is the full ordered list of choices. */
export interface SaveCellTypeRequest {
  name: string;
  kind: CellKind;
  description: string | null;
  maxLength: number | null;
  decimalPlaces: number | null;
  minValue: number | null;
  maxValue: number | null;
  unit: string | null;
  options: string[];
}
