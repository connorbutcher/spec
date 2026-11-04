/** One version of a template, for the version picker. A version in use on a sheet is read-only. */
export type TemplateVersionSummary = {
  id: number;
  versionNumber: number;
  createdAtUtc: string;
  isInUse: boolean;
}
