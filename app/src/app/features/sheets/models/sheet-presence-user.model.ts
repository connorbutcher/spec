/** Someone who has the sheet open right now, in `connectionCount` tabs or windows. */
export type SheetPresenceUser = {
  userId: number;
  displayName: string;
  connectionCount: number;
}
