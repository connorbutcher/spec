import { PropertyValue } from '../../../domain/spec-sheet/properties/values/property-value';

export type CreateSpecSheetPropertyValueRequest = {
  specSheetTypePropertyId: number;
  propertyValue: PropertyValue;
};
