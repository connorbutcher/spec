import { StructureLevelType } from '../../../types/configuration/structure-level-type.enum';
import { SpecSheetTypeStructureLevelValue } from './spec-sheet-type-structure-level-value';

export type SpecSheetTypeStructureLevel = {
  id: number;
  specSheetTypeId: number;
  structureLevelType: StructureLevelType;
  level: number;
  sortOrientation: number;
  levelValues: SpecSheetTypeStructureLevelValue[];
};
