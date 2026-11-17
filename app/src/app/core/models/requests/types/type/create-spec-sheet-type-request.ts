import { TableOrientation } from '../../../domain/types/spec-sheet-type';
import { CreateSpecSheetTypePropertyRequest } from '../properties/create-spec-sheet-type-property-request';
import { CreateSpecSheetTypeStructureLevelRequest } from '../structure/create-spec-sheet-type-structure-level-request';

export type CreateSpecSheetTypeRequest = {
  type: string;
  displayOrder: number;
  tableOrientation: TableOrientation;
  canCopyFromMigrated: boolean;
  requiresApprovalWorkflow: boolean;
  specSheetPhaseIds: number[];
  properties: CreateSpecSheetTypePropertyRequest[];
  structureLevels: CreateSpecSheetTypeStructureLevelRequest[];
};
