import { CellKind } from './cell-kind';

/** How a cell kind is presented in the designer. */
export interface CellKindInfo {
  kind: CellKind;
  label: string;
  icon: string;
  description: string;
}
