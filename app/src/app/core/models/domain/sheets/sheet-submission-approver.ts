import { User } from '../access/user';

export type SheetSubmissionApprover = {
  sheetSubmissionId: number;
  userId: number;
  user?: User;
  decision: number;
  decidedDateTime?: string;
  reason?: string;
};
