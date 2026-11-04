/** The published version in which something last changed, and who changed it. */
export type SheetChange = {
  versionNumber: number;
  atUtc: string;
  userName: string;
}
