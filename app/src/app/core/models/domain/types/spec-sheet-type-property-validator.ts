import { PropertyValidatorType } from '../../../types/properties/property-validator-type.enum';
import { ValidatorProperty } from '../configuration/validator-properties/validator-property';

export type SpecSheetTypePropertyValidator = {
  validator: PropertyValidatorType;
  message: string;
  property?: ValidatorProperty;
};
