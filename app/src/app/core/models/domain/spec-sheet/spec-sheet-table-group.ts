import { User } from '../access/user';
import { SpecSheetTableGroupWidget } from './spec-sheet-table-group-widget';
import { SpecSheetTableRow } from './spec-sheet-table-row';

export type SpecSheetTableGroup = {
  id: number;
  specSheetTableId: number;
  linkedSpecSheetTableGroupId?: number;
  tableGroupTemplateId: number;
  order: number;
  referenceId: string;
  isDeleted: boolean;
  rows: SpecSheetTableRow[];
  widgets: SpecSheetTableGroupWidget[];
  createdByUserId: number;
  createdDateTime: string;
  createdByUser?: User;
  lastModifiedByUserId?: number;
  lastModifiedDateTime?: string;
  lastModifiedByUser?: User;
};
