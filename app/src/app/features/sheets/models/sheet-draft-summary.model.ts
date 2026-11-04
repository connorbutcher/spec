/** How many unpublished changes one person has on a sheet. */
export type SheetDraftSummary = {
  userId: number;
  userName: string;
  draftCount: number;
}
