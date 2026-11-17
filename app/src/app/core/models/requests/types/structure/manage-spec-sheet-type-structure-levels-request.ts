import { CreateSpecSheetTypeStructureLevelValueRequest } from './create-spec-sheet-type-structure-level-value-request';
import { UpdateSpecSheetTypeStructureLevelValueRequest } from './update-spec-sheet-type-structure-level-value-request';

export type ManageSpecSheetTypeStructureLevelValueRequest = {
  createLevelValueRequests: CreateSpecSheetTypeStructureLevelValueRequest[];
  updateLevelValueRequests: UpdateSpecSheetTypeStructureLevelValueRequest[];
  deleteLevelValueRequests: number[];
};
