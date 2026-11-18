import { SheetItemRevision } from './sheet-item-revision';
import { SheetTableRow } from './sheet-table-row';
import { SheetWidget } from './sheet-widget';

export type SheetTableGroup = {
  id: number;
  sheetTableId: number;
  tableGroupTemplateId: number;
  referenceId: string;
  createdDateTime: string;
  revision: SheetItemRevision;
  displayOrder: number;
  linkedGroupReferenceId?: string;
  rows: SheetTableRow[];
  widgets: SheetWidget[];
};
