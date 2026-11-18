import { PropertyValue } from '../../domain/values/property-values/property-value';

export type CreateSheetPropertyValueRequest = {
  specSheetTypePropertyId: number;
  propertyValue: PropertyValue;
};
