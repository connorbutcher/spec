import { RowTakeoverStatus } from './row-takeover-status';

/**
 * One person asking to take over a row that is checked out to another. The holder can approve or deny
 * it; left unanswered until `expiresAtUtc`, it is granted. Only a row the holder has not changed can
 * change hands: once it has unpublished changes it stays with them until they publish or discard.
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
