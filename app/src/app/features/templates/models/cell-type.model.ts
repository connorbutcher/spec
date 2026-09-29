import { CellConfiguration } from './cell-configuration';
import { CellKind } from './cell-kind';
import { CellStyle } from './cell-style.model';
import { CellTypeOption } from './cell-type-option.model';

/** A user-defined cell type, with the default configuration and style its cells start with. */
export interface CellType {
  id: number;
  name: string;
  kind: CellKind;
  description: string | null;
  displayOrder: number;
  /** Always for `kind`. */
  configuration: CellConfiguration;
  style: CellStyle;
  options: CellTypeOption[];
  /** How many template cells use this type. A type in use can't be deleted. */
  usageCount: number;
}
