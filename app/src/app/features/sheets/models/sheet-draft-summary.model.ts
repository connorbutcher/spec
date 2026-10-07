/** How many unpublished changes one person has on a sheet. */
export interface SheetDraftSummary {
  userId: number;
  userName: string;
  draftCount: number;
}
