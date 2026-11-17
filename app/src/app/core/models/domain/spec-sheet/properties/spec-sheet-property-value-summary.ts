import { PropertyControlType } from '../../../../types/properties/property-control-type.enum';
import { PropertyValue } from './values/property-value';

export type SpecSheetPropertyValueSummary = {
  propertyValue: PropertyValue;
  propertyKey: string;
  propertyName: string;
  referenceId: string;
  valueControlType: PropertyControlType;
};
