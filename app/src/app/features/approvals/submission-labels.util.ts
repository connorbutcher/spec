import { SheetLabelsUtil } from '../sheets/status/sheet-labels.util';
import { ApprovalDecision } from './models/approval-decision';
import { SheetSubmission } from './models/sheet-submission.model';
import { SubmissionChangeCounts } from './models/submission-change-counts.model';
import { SubmissionStatus } from './models/submission-status';

const STATUS_LABELS: Readonly<Record<SubmissionStatus, string>> = {
  Pending: 'Waiting for approval',
  Approved: 'Approved',
  Rejected: 'Rejected',
  Withdrawn: 'Withdrawn',
};

const DECISION_ICONS: Readonly<Record<ApprovalDecision, string>> = {
  Pending: 'pi pi-clock',
  Approved: 'pi pi-check',
  Rejected: 'pi pi-times',
};

export class SubmissionLabelsUtil {
  /** "V6 Parts": the sheet a submission is for. */
  public static sheetLabel(submission: SheetSubmission): string {
    return `${submission.phaseCode} ${submission.sheetTypeName}`;
  }

  /** "2 rows, 1 section": what a submission changes, or "No changes" once it holds none. */
  public static changesLabel(counts: SubmissionChangeCounts): string {
    const parts = [
      SubmissionLabelsUtil.count(counts.tables, 'table'),
      SubmissionLabelsUtil.count(counts.sections, 'section'),
      SubmissionLabelsUtil.count(counts.rows, 'row'),
      SubmissionLabelsUtil.count(counts.columnBlocks, 'column'),
    ].filter((part) => part !== null);
    return parts.length === 0 ? 'No changes' : parts.join(', ');
  }

  /** "v7" for an approved submission's version; "Waiting for approval", "Rejected" or "Withdrawn" otherwise. */
  public static statusLabel(submission: SheetSubmission): string {
    if (submission.status === 'Approved' && submission.versionNumber !== null) {
      return `Approved · ${SheetLabelsUtil.versionLabel(submission.versionNumber)}`;
    }
    return STATUS_LABELS[submission.status];
  }

  public static decisionIcon(decision: ApprovalDecision): string {
    return DECISION_ICONS[decision];
  }

  /** "12 Sep 2026, 14:05": when it was submitted. */
  public static submittedLabel(submission: SheetSubmission): string {
    return SheetLabelsUtil.momentLabel(submission.submittedAtUtc);
  }

  /** The reason the submission was rejected, with who rejected it, or null. */
  public static rejection(submission: SheetSubmission): string | null {
    const rejecter = submission.approvers.find((approver) => approver.decision === 'Rejected');
    return rejecter?.reason ? `${rejecter.userName}: ${rejecter.reason}` : null;
  }

  /** The route to the sheet showing a pending submission's changes for review. */
  public static reviewLink(submission: SheetSubmission): unknown[] {
    return ['/phases', submission.phaseId, 'sheets', submission.sheetTypeId];
  }

  private static count(value: number, noun: string): string | null {
    return value === 0 ? null : `${value} ${noun}${value === 1 ? '' : 's'}`;
  }
}
