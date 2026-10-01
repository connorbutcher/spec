/** The published version in which something last changed, and who changed it. */
export interface SheetChange {
  versionNumber: number;
  atUtc: string;
  userName: string;
}
