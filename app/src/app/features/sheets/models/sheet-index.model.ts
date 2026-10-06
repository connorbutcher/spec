import { SheetCell } from './sheet-cell.model';
import { SheetRow } from './sheet-row.model';
import { SheetSection } from './sheet-section.model';
import { SheetTable } from './sheet-table.model';

/** Every table, section, row and cell of a sheet by id, with where each one sits, for looking things up. */
export interface SheetIndex {
  tables: ReadonlyMap<number, SheetTable>;
  sections: ReadonlyMap<number, SheetSection>;
  rows: ReadonlyMap<number, SheetRow>;
  cells: ReadonlyMap<number, SheetCell>;
  /** A section's parent section, or null for a top-level one. */
  sectionParent: ReadonlyMap<number, number | null>;
  sectionTable: ReadonlyMap<number, number>;
  rowSection: ReadonlyMap<number, number>;
}
