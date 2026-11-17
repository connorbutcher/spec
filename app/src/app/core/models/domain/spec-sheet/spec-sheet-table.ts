import { User } from '../access/user';
import { TableProperties } from '../properties/table/table-properties';
import { SpecSheetTableGroup } from './spec-sheet-table-group';
import { SpecSheetTableNote } from './spec-sheet-table-note';
import { SpecSheetTableWidget } from './spec-sheet-table-widget';

export type SpecSheetTable = {
  id: number;
  specSheetId: number;
  tableTemplateId: number;
  title: string;
  displayOrder: number;
  referenceId: string;
  isDeleted: boolean;
  groups: SpecSheetTableGroup[];
  widgets: SpecSheetTableWidget[];
  notes: SpecSheetTableNote[];
  createdByUserId: number;
  createdDateTime: string;
  createdByUser?: User;
  lastModifiedByUserId?: number;
  lastModifiedDateTime?: string;
  lastModifiedByUser?: User;
  configurationOverride?: TableProperties;
};
