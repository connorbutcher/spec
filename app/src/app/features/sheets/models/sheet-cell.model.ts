import { TemplateCell } from '../../templates/models/template-cell.model';
import { CellInstanceSettings } from './cell-instance-settings';
import { CellValue } from './cell-value';
import { SheetChange } from './sheet-change.model';

/**
 * A cell on a sheet. `template` is the layout it was built from (its own `id` is the template cell's).
 * Read its value through `CellValueUtil`, which gives it in the type its kind takes.
 */
export type SheetCell = {
  id: number;
  publicId: string;
  template: TemplateCell;
  /** The cell's value, in the shape its kind takes; null when it holds nothing. */
  value: CellValue;
  /** The settings chosen for this cell on the sheet, for a kind that has them; null when nothing is chosen. */
  settings: CellInstanceSettings | null;

  /** The column block copy the cell belongs to, or null for the row's own cells. */
  sheetColumnBlockId: number | null;
  /** The publish that last changed this value. */
  lastChange: SheetChange | null;
}
