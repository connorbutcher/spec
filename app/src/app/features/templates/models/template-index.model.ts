import { CellEntry } from './cell-entry.model';
import { ColumnBlockEntry } from './column-block-entry.model';
import { RowEntry } from './row-entry.model';
import { SectionEntry } from './section-entry.model';

/** Lookups by id over a template's sections, column blocks, rows and cells. */
export interface TemplateIndex {
  sections: ReadonlyMap<number, SectionEntry>;
  columnBlocks: ReadonlyMap<number, ColumnBlockEntry>;
  rows: ReadonlyMap<number, RowEntry>;
  cells: ReadonlyMap<number, CellEntry>;
}
