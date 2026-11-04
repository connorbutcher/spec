import { RowEntry } from './row-entry.model';
import { TemplateCell } from './template-cell.model';

/** A cell with the row (and so section) it belongs to. */
export type CellEntry = {
  cell: TemplateCell;
  row: RowEntry;
}
