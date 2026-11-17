import { SpecSheetExternalReference } from '../../domain/references/spec-sheet-external-reference';
import { ManageSpecSheetTablesRequest } from './tables/manage-spec-sheet-tables-request';

export type UpdateSpecSheetRequest = {
  id: number;
  manageSpecSheetTablesRequest: ManageSpecSheetTablesRequest;
  systemReferences: SpecSheetExternalReference[];
};
