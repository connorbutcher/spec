/** Who holds a draft on a table, section or row. While it exists, only that person can change the item. */
export type SheetLock = {
  userId: number;
  userName: string;
  isMine: boolean;
  /**
   * True when the person is only in the row and has changed nothing, so nothing is saved for it. Set on
   * this side from the live connection (`RowCheckoutStore`); a lock that came with the sheet never has it.
   */
  isCheckout?: boolean;
  /**
   * The change has been submitted for publishing and waits for approval: nobody can change the item, the
   * person who made it included, until it is approved and published or rejected and handed back.
   */
  isAwaitingApproval?: boolean;
}
