import { TemplateCell } from './template-cell.model';

/** A row of a section, with its cells ordered by column. */
export type TemplateRow = {
  id: number;
  displayOrder: number;
  cells: TemplateCell[];
}
