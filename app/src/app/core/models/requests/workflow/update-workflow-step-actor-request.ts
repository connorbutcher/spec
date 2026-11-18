import { WorkflowState } from '../../domain/enums/workflow-state';

export type UpdateWorkflowStepActorRequest = {
  specSheetId: number;
  workflowStepActorId: number;
  workflowStepId: number;
  state: WorkflowState;
  comment?: string;
};
