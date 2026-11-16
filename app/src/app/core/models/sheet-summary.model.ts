import { SheetIdentifierValue } from './sheet-identifier-value.model';

/** One sheet in a phase's list of sheets: what tells it apart, and where its versions and drafts stand. */
export type SheetSummary = {
  id: number;
  publicId: string;
  phaseId: number;
  sheetTypeId: number;
  /** An identifier the sheet has no value for is left out. */
  identifiers: SheetIdentifierValue[];
  latestVersionNumber: number | null;
  latestPublishedAtUtc: string | null;
  createdAtUtc: string;
  /** Unpublished changes on the sheet, by anyone. */
  draftCount: number;
  pendingSubmissionCount: number;
}
