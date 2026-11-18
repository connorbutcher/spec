import { ApproverDecision } from '../../../types/sheets/approver-decision.enum';

export type DecideSubmissionRequest = {
  sheetId: number;
  submissionId: number;
  decision: ApproverDecision;
  comment?: string;
};
