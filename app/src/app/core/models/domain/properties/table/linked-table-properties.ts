import { TableProperties } from './table-properties';

export type LinkedTableProperties = TableProperties & {
  specSheetTypeId?: number;
  specSheetPhaseId?: number;
  tableReferenceId?: string;
  lockToLatestPublished?: boolean;
};
