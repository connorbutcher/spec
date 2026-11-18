import { CreateSheetPropertyValueRequest } from './create-sheet-property-value-request';

export type CreateSheetRequest = {
  specSheetPhaseId: number;
  specSheetTypeId: number;
  propertyValues: CreateSheetPropertyValueRequest[];
};
