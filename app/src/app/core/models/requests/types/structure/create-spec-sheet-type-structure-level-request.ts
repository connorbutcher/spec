import { StructureLevelType } from '../../../../types/configuration/structure-level-type.enum';
import { CreateSpecSheetTypeStructureLevelValueRequest } from './create-spec-sheet-type-structure-level-value-request';

export type CreateSpecSheetTypeStructureLevelRequest = {
  specSheetTypeId: number;
  structureLevelType: StructureLevelType;
  level: number;
  sortOrientation: number;
  valueRequests: CreateSpecSheetTypeStructureLevelValueRequest[];
};
