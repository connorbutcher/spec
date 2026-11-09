import { ApprovalDecision } from './approval-decision';

/** One of a submission's approvers and what they decided; `reason` is set for a rejection. */
export type SubmissionApprover = {
  userId: number;
  userName: string;
  decision: ApprovalDecision;
  decidedAtUtc: string | null;
  reason: string | null;
}
