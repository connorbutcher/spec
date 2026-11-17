import { PropertyChanged } from '../../../common/property-changed';
import { SpecSheetTypePropertyValidator } from '../../../domain/types/spec-sheet-type-property-validator';

export type UpdateSpecSheetTypePropertyRequest = {
  id: number;
  valueName?: string;
  valueKey?: string;
  valueType?: string;
  placeholderText?: PropertyChanged<string | undefined>;
  valueControlType?: string;
  canValueBeSetInForm?: boolean;
  displayOrder?: number;
  validators?: PropertyChanged<SpecSheetTypePropertyValidator[] | undefined>;
};
