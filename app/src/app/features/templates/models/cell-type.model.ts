import { CellKind } from './cell-kind';
import { CellTypeOption } from './cell-type-option.model';

/** A user-defined cell type. Only the settings for its kind are set; the rest are null. */
export interface CellType {
  id: number;
  name: string;
  kind: CellKind;
  description: string | null;
  displayOrder: number;
  maxLength: number | null;
  decimalPlaces: number | null;
  minValue: number | null;
  maxValue: number | null;
  unit: string | null;
  options: CellTypeOption[];
  /** How many template cells use this type. A type in use can't be deleted. */
  usageCount: number;
}
