import { CellEntry } from './cell-entry.model';
import { RowEntry } from './row-entry.model';
import { SectionEntry } from './section-entry.model';

/** Lookups by id over a template's sections, rows and cells. */
export interface TemplateIndex {
  sections: ReadonlyMap<number, SectionEntry>;
  rows: ReadonlyMap<number, RowEntry>;
  cells: ReadonlyMap<number, CellEntry>;
}
