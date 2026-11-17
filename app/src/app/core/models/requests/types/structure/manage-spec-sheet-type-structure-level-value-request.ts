import { CreateSpecSheetTypeStructureLevelRequest } from './create-spec-sheet-type-structure-level-request';
import { UpdateSpecSheetTypeStructureLevelRequest } from './update-spec-sheet-type-structure-level-request';

export type ManageSpecSheetTypeStructureLevelsRequest = {
  createLevelRequests: CreateSpecSheetTypeStructureLevelRequest[];
  updateLevelRequests: UpdateSpecSheetTypeStructureLevelRequest[];
  deleteLevelRequests: number[];
};
