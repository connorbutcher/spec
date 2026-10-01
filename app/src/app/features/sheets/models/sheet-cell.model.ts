import { TemplateCell } from '../../templates/models/template-cell.model';

/**
 * A cell on a sheet. `template` is the layout it was built from (its own `id` is the template cell's),
 * and only the value field that matches the cell's kind is ever set.
 */
export interface SheetCell {
  id: number;
  publicId: string;
  template: TemplateCell;
  textValue: string | null;
  numberValue: number | null;
  /** A date as `yyyy-MM-dd`. */
  dateValue: string | null;
  booleanValue: boolean | null;
  optionId: number | null;
}
