import { CreateSpecSheetPropertyValueRequest } from './properties/create-spec-sheet-property-value-request';

export type CreateSpecSheetRequest = {
  specSheetPhaseId: number;
  specSheetTypeId: number;
  propertyValues: CreateSpecSheetPropertyValueRequest[];
};
