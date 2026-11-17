import { PropertyControlType } from '../../../types/properties/property-control-type.enum';
import { PropertyValueType } from '../../../types/properties/property-value-type.enum';
import { SpecSheetTypePropertyValidator } from './spec-sheet-type-property-validator';

export type SpecSheetTypeProperty = {
  id: number;
  specSheetTypeId: number;
  referenceId: string;
  valueName: string;
  valueKey: string;
  valueType: PropertyValueType;
  placeholderText?: string;
  valueControlType: PropertyControlType;
  canValueBeSetInForm: boolean;
  displayOrder: number;
  saveValueAsProperty: boolean;
  validators?: SpecSheetTypePropertyValidator[];
};
