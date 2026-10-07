import { RowTakeoverStatus } from './row-takeover-status';

/**
 * One person asking to take over a row that is checked out to another. The holder can approve or deny
 * it; left unanswered until `expiresAtUtc`, it is granted. The row changes hands with the holder's
 * unpublished changes in it.
 */
export interface RowTakeover {
  id: string;
  sheetId: number;
  rowId: number;
  requesterUserId: number;
  requesterName: string;
  holderUserId: number;
  holderName: string;
  requestedAtUtc: string;
  expiresAtUtc: string;
  status: RowTakeoverStatus;
}
