import { CreateSpecSheetTypePropertyRequest } from './create-spec-sheet-type-property-request';
import { UpdateSpecSheetTypePropertyRequest } from './update-spec-sheet-type-property-request';

export type ManageSpecSheetTypePropertiesRequest = {
  specSheetTypeId: number;
  createPropertyRequests: CreateSpecSheetTypePropertyRequest[];
  updatePropertyRequests: UpdateSpecSheetTypePropertyRequest[];
  deletePropertyRequests: number[];
};
