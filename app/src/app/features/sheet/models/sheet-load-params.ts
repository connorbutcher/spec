/** Which sheet the sheet page reads, and whether as it stands now or as a published version. */
export interface SheetLoadParams {
  sheetId: number;
  versionNumber: number | undefined;
  isDraft: boolean;
}
