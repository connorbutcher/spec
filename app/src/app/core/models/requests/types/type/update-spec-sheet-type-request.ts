import { ManageSpecSheetTypePropertiesRequest } from '../properties/manage-spec-sheet-type-property-request';
import { ManageSpecSheetTypeStructureLevelsRequest } from '../structure/manage-spec-sheet-type-structure-level-value-request';

export type UpdateSpecSheetTypeRequest = {
  id: number;
  type: string;
  displayOrder: number;
  canCopyFromMigrated: boolean;
  requiresApprovalWorkflow: boolean;
  newSpecSheetPhaseIds: number[];
  removedSpecSheetPhaseIds: number[];
  properties?: ManageSpecSheetTypePropertiesRequest;
  structure?: ManageSpecSheetTypeStructureLevelsRequest;
};
