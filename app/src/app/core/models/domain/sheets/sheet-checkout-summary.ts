/** How many of a sheet's rows and tables are checked out, by the current user and by others. */
export type SheetCheckoutSummary = {
  /** The current user's checked-out items that can still be changed, undone or submitted. */
  mine: number;
  /** The current user's items awaiting approval. */
  mineAwaitingApproval: number;
  /** Everyone else's checked-out or awaiting items, by their name. */
  others: { userName: string; count: number }[];
};
