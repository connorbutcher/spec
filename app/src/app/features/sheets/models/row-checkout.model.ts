/**
 * A row someone has clicked into but not changed yet. It is theirs while they stay in it. Nothing is
 * saved for it, so it arrives over the live connection rather than in the sheet.
 */
export interface RowCheckout {
  rowId: number;
  userId: number;
  userName: string;
}
