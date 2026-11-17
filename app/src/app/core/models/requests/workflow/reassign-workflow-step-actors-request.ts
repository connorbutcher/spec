export type ReassignWorkflowStepActorsRequest = {
  specSheetId: number;
  workflowStepId: number;
  createWorkflowStepActorRequests: number[];
  deleteWorkflowStepActorRequests: number[];
};
