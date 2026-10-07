import { AvailableTemplate } from './available-template.model';
import { SheetDraftSummary } from './sheet-draft-summary.model';
import { SheetTable } from './sheet-table.model';
import { SheetVersionSummary } from './sheet-version-summary.model';

/**
 * A sheet as one viewer sees it. A live view is the latest published state with the viewer's own drafts
 * on top and other people's locks marked; a past view (by version number or date) is read-only.
 */
export interface Sheet {
  id: number;
  publicId: string;
  phaseId: number;
  sheetTypeId: number;
  isLive: boolean;
  viewedVersionNumber: number | null;
  viewedAsOfUtc: string | null;
  latestVersionNumber: number | null;
  myDraftCount: number;
  /** Other people's unpublished changes, by person. Publishing everything takes these too. */
  otherDrafts: SheetDraftSummary[];
  versions: SheetVersionSummary[];
  tables: SheetTable[];
  availableTemplates: AvailableTemplate[];
}
