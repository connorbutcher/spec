/** One publish of a sheet. Viewing it shows the sheet as it stood when it was published. */
export interface SheetVersionSummary {
  versionNumber: number;
  publishedAtUtc: string;
  publishedByUserId: number;
  publishedByName: string;
  note: string | null;
}
