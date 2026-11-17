import { User } from '../access/user';
import { WorkflowState } from '../enums/workflow-state';

export type WorkflowStepActor = {
  id: number;
  workflowStepId: number;
  state: WorkflowState;
  comment?: string;
  actionedDateTime?: string;
  actor?: User;
};
