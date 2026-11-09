import { SubmissionApprover } from './submission-approver.model';
import { SubmissionChangeCounts } from './submission-change-counts.model';
import { SubmissionStatus } from './submission-status';

/**
 * A publish waiting for approval, or one that was decided. Its changes are published only once every
 * approver approves; `versionNumber` is the version that published them.
 */
export type SheetSubmission = {
  id: number;
  sheetId: number;
  phaseId: number;
  phaseCode: string;
  sheetTypeId: number;
  sheetTypeName: string;
  note: string | null;
  submittedByUserId: number;
  submittedByName: string;
  submittedAtUtc: string;
  status: SubmissionStatus;
  decidedAtUtc: string | null;
  versionNumber: number | null;
  changes: SubmissionChangeCounts;
  approvers: SubmissionApprover[];
}
