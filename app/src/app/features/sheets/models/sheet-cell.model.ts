import { TemplateCell } from '../../templates/models/template-cell.model';

/**
 * A cell on a sheet. `template` is the layout it was built from (its own `id` is the template cell's),
 * and only the value field that matches the cell's kind is ever set.
 */
import { SheetChange } from './sheet-change.model';

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
  /** The column block copy the cell belongs to, or null for the row's own cells. */
  sheetColumnBlockId: number | null;
  /** The publish that last changed this value. */
  lastChange: SheetChange | null;
}
