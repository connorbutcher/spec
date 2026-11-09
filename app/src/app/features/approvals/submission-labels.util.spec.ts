import { SheetSubmission } from './models/sheet-submission.model';
import { SubmissionLabelsUtil } from './submission-labels.util';

const SUBMISSION: SheetSubmission = {
  id: 12,
  sheetId: 4,
  phaseId: 3,
  phaseCode: 'V6',
  sheetTypeId: 2,
  sheetTypeName: 'Parts',
  note: null,
  submittedByUserId: 1,
  submittedByName: 'Developer',
  submittedAtUtc: '2026-11-09T10:00:00Z',
  status: 'Pending',
  decidedAtUtc: null,
  versionNumber: null,
  changes: { tables: 0, sections: 1, rows: 2, columnBlocks: 0, total: 3 },
  approvers: [
    { userId: 2, userName: 'Engineer Two', decision: 'Approved', decidedAtUtc: '2026-11-09T11:00:00Z', reason: null },
    { userId: 3, userName: 'Engineer Three', decision: 'Pending', decidedAtUtc: null, reason: null },
  ],
};

describe('SubmissionLabelsUtil', () => {
  it('names the sheet and what the submission changes', () => {
    expect(SubmissionLabelsUtil.sheetLabel(SUBMISSION)).toBe('V6 Parts');
    expect(SubmissionLabelsUtil.changesLabel(SUBMISSION.changes)).toBe('1 section, 2 rows');
    expect(SubmissionLabelsUtil.changesLabel({ tables: 0, sections: 0, rows: 0, columnBlocks: 0, total: 0 })).toBe('No changes');
  });

  it('says where it stands, with the version an approval published', () => {
    expect(SubmissionLabelsUtil.statusLabel(SUBMISSION)).toBe('Waiting for approval');
    expect(SubmissionLabelsUtil.statusLabel({ ...SUBMISSION, status: 'Approved', versionNumber: 7 })).toBe('Approved · v7');
  });

  it('gives who rejected it and why', () => {
    const rejected: SheetSubmission = {
      ...SUBMISSION,
      status: 'Rejected',
      approvers: [{ ...SUBMISSION.approvers[1], decision: 'Rejected', reason: 'Torque is out of range' }],
    };
    expect(SubmissionLabelsUtil.rejection(rejected)).toBe('Engineer Three: Torque is out of range');
    expect(SubmissionLabelsUtil.rejection(SUBMISSION)).toBeNull();
  });
});
