import { User } from '../access/user';
import { SpecSheetTableCell } from './spec-sheet-table-cell';

export type SpecSheetTableRow = {
  id: number;
  specSheetTableGroupId: number;
  tableRowTemplateId: number;
  rowNumber: number;
  referenceId: string;
  isDeleted: boolean;
  cells: SpecSheetTableCell[];
  createdByUserId: number;
  createdDateTime: string;
  createdByUser?: User;
  lastModifiedByUserId?: number;
  lastModifiedDateTime?: string;
  lastModifiedByUser?: User;
};
