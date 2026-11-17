import { SpecSheetTypeProperty } from '../../types/spec-sheet-type-property';
import { PropertyValue } from './values/property-value';

export type SpecSheetPropertyValue = {
  id: number;
  specSheetId: number;
  specSheetTypePropertyId: number;
  propertyValue?: PropertyValue;
  specSheetTypeProperty: SpecSheetTypeProperty;
};
