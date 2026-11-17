import { ManageSpecSheetTypeStructureLevelValueRequest } from './manage-spec-sheet-type-structure-levels-request';

export type UpdateSpecSheetTypeStructureLevelRequest = {
  id: number;
  level: number;
  sortOrientation: number;
  manageLevelValueRequest?: ManageSpecSheetTypeStructureLevelValueRequest;
};
