import { ApproverDecision } from '../../../types/sheets/approver-decision.enum';

export type DecideSubmissionRequest = {
  sheetId: number;
  decision: ApproverDecision;
  comment?: string;
};
