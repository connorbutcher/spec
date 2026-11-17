import { WorkflowState } from '../enums/workflow-state';
import { WorkflowStepActor } from './workflow-step-actor';

export type WorkflowStep = {
  id: number;
  specSheetId: number;
  startedDateTime: string;
  state: WorkflowState;
  actors: WorkflowStepActor[];
};
