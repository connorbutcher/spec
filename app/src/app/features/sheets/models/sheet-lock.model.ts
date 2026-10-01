/** Who holds a draft on a table, section or row. While it exists, only that person can change the item. */
export interface SheetLock {
  userId: number;
  userName: string;
  isMine: boolean;
}
