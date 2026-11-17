import { WorkflowState } from '../../domain/enums/workflow-state';

export type UpdateWorkflowStepActorRequest = {
  workflowStepActorId: number;
  workflowStepId: number;
  state: WorkflowState;
  comment?: string;
};
