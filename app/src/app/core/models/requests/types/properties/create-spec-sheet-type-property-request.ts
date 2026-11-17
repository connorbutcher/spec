import { SpecSheetTypePropertyValidator } from '../../../domain/types/spec-sheet-type-property-validator';

export type CreateSpecSheetTypePropertyRequest = {
  specSheetTypeId: number;
  referenceId: string;
  valueName: string;
  valueKey: string;
  valueType: string;
  placeholderText?: string;
  valueControlType: string;
  canValueBeSetInForm: boolean;
  displayOrder: number;
  validators: SpecSheetTypePropertyValidator[];
};
