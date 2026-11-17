import { User } from '../access/user';
import { SpecSheetExternalReference } from '../references/spec-sheet-external-reference';
import { SpecSheetPhase } from '../types/spec-sheet-phase';
import { SpecSheetType } from '../types/spec-sheet-type';
import { WorkflowStep } from '../workflow/workflow-step';
import { SpecSheetPropertyValue } from './properties/spec-sheet-property-value';
import { SpecSheetTable } from './spec-sheet-table';

export type SpecSheet = {
  id: number;
  specSheetPhaseId: number;
  specSheetTypeId: number;
  versionNumber: number;
  isPublished: boolean;
  createdByUserId: number;
  createdDateTime: string;
  createdByUser: User;
  hasBeenMigrated: boolean;
  publishedByUserId?: number;
  publishedDateTime?: string;
  publishedByUser?: User;
  changeLog?: string;
  specSheetPhase: SpecSheetPhase;
  specSheetType: SpecSheetType;
  tables: SpecSheetTable[];
  propertyValues: SpecSheetPropertyValue[];
  workflowSteps: WorkflowStep[];
  externalReferences: SpecSheetExternalReference[];
};
