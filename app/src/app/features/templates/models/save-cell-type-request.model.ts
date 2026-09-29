import { CellConfiguration } from './cell-configuration';
import { CellKind } from './cell-kind';
import { CellStyle } from './cell-style.model';

/**
 * The body for creating or replacing a cell type. A configuration for another kind is dropped, so a
 * kind change starts with nothing set. `options` is the full ordered list of dropdown choices.
 */
export interface SaveCellTypeRequest {
  name: string;
  kind: CellKind;
  description: string | null;
  configuration: CellConfiguration | null;
  style: CellStyle | null;
  options: string[];
}
