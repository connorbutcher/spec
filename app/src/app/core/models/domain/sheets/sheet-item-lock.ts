/**
 * Who has a sheet item (table, group or row) checked out: the user whose draft it shows. Only they can change it, and
 * nobody can while it is awaiting approval.
 */
export type SheetItemLock = {
  userId: number;
  userName: string;
  isMine: boolean;
  isAwaitingApproval: boolean;
};
