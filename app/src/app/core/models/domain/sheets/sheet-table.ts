import { TableProperties } from '../properties/table/table-properties';
import { SheetItemRevision } from './sheet-item-revision';
import { SheetWidget } from './sheet-widget';
import { SheetTableGroup } from './sheet-table-group';
import { SheetTableNote } from './sheet-table-note';

export type SheetTable = {
  id: number;
  sheetId: number;
  tableTemplateId: number;
  referenceId: string;
  createdDateTime: string;
  revision: SheetItemRevision;
  title: string;
  displayOrder: number;
  configurationOverride?: TableProperties;
  groups: SheetTableGroup[];
  widgets: SheetWidget[];
  notes: SheetTableNote[];
};
