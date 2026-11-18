import { User } from '../access/user';
import { SheetSubmissionApprover } from './sheet-submission-approver';

export type SheetSubmission = {
  id: number;
  sheetId: number;
  note?: string;
  submittedByUserId: number;
  submittedByUser?: User;
  submittedDateTime: string;
  status: number;
  decidedDateTime?: string;
  sheetVersionId?: number;
  sheetVersionNumber?: number;
  approvers: SheetSubmissionApprover[];
};
