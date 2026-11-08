import { TemplateCell } from '../../templates/models/template-cell.model';
import { SheetRowsChange } from './sheet-rows-change.model';

/** Rows as the API sends them: each cell names its template cell, listed once in `templateCells`. */
export type SheetRowsChangeResponse = {
  templateCells: TemplateCell[];
} & SheetRowsChange
