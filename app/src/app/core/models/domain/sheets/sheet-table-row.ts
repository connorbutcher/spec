import { SheetItemRevision } from './sheet-item-revision';
import { SheetTableCell } from './sheet-table-cell';

export type SheetTableRow = {
  id: number;
  sheetTableGroupId: number;
  tableRowTemplateId: number;
  referenceId: string;
  createdDateTime: string;
  revision: SheetItemRevision;
  displayOrder: number;
  cells: SheetTableCell[];
};
