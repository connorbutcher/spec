import { TemplateOrientation } from '../../templates/models/template-orientation';
import { AddableColumnBlock } from './addable-column-block.model';
import { AddableSection } from './addable-section.model';
import { SheetColumnBlock } from './sheet-column-block.model';
import { SheetLinkableColumn } from './sheet-linkable-column.model';
import { SheetLock } from './sheet-lock.model';
import { SheetSection } from './sheet-section.model';

/** A table on a sheet, built from one version of a table template and keeping that layout. */
export interface SheetTable {
  id: number;
  publicId: string;
  tableTemplateId: number;
  templateName: string;
  templateVersionNumber: number;
  orientation: TemplateOrientation;
  /** How many of the table's own columns, from the left, stay pinned while the rest scroll. */
  stickyColumnCount: number;
  title: string | null;
  displayOrder: number;
  lock: SheetLock | null;
  isPending: boolean;
  sections: SheetSection[];
  addableSections: AddableSection[];
  /** The copies of the template's column blocks, left to right; empty for a vertical table. */
  columnBlocks: SheetColumnBlock[];
  addableColumnBlocks: AddableColumnBlock[];
  /** The table's columns a linked dropdown on the sheet can take its choices from. */
  linkableColumns: SheetLinkableColumn[];

}
