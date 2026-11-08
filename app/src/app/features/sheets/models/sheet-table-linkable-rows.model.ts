import { SheetLinkableRow } from './sheet-linkable-row.model';

/** The rows of one table that a linked dropdown on the sheet can take its choices from, top to bottom. */
export type SheetTableLinkableRows = {
  sheetTableId: number;
  rows: SheetLinkableRow[];
}
