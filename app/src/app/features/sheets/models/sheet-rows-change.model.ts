import { SheetDraftSummary } from './sheet-draft-summary.model';
import { SheetLinkedSource } from './sheet-linked-source.model';
import { SheetRow } from './sheet-row.model';
import { SheetTableLinkableRows } from './sheet-table-linkable-rows.model';

/**
 * Some rows of the live sheet as the viewer now sees them, with the parts of the sheet that follow from
 * them. Saving a row answers with this, and it is read when someone else checks rows out or releases
 * them, in place of the whole sheet. A row asked for and not listed has gone from the sheet.
 */
export type SheetRowsChange = {
  sheetId: number;
  rows: SheetRow[];
  /** The sheet's newest version: when it differs from the open copy's, only a whole read will do. */
  latestVersionNumber: number | null;
  myDraftCount: number;
  otherDrafts: SheetDraftSummary[];
  linkedSources: SheetLinkedSource[];
  /** For each table the rows are in, its rows a linked dropdown can take its choices from. */
  linkableRows: SheetTableLinkableRows[];
}
