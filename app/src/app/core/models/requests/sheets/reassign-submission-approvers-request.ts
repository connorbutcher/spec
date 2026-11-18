export type ReassignSubmissionApproversRequest = {
  sheetId: number;
  submissionId: number;
  addedUserIds: number[];
  removedUserIds: number[];
};
