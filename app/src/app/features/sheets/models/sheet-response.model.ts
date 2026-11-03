import { TemplateCell } from '../../templates/models/template-cell.model';
import { Sheet } from './sheet.model';

/**
 * A sheet as the API sends it. Many cells share a template cell, so each cell names its own by
 * `templateCellId` (see `SheetCellTemplateRef`) in place of carrying it, and `templateCells` lists each
 * one once. `withTemplateCells` gives every cell its `template` back.
 */
export interface SheetResponse extends Sheet {
  templateCells: TemplateCell[];
}
