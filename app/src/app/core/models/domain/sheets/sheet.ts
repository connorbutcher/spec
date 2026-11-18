import { User } from '../access/user';
import { CellConfiguration } from '../configuration/cell-configuration';
import { CellProperties } from '../properties/cell/cell-properties';
import { TableProperties } from '../properties/table/table-properties';
import { SpecSheetTypeProperty } from '../types/spec-sheet-type-property';
import { SpecSheetPhase } from '../types/spec-sheet-phase';
import { SpecSheetType } from '../types/spec-sheet-type';
import { TableCellType } from '../template/table-cell-type';
import { WidgetValue } from '../values/widget-values/widget-value';
import { NoteSourceSystem } from '../spec-sheet/spec-sheet-table-note';

/**
 * A sheet in the shape the sheets endpoints return it: every item has its own id and the revision of it that is shown.
 */
export type SheetItemRevision = {
  id: number;
  revisionNumber: number;
  status: number;
  createdByUserId: number;
  createdDateTime: string;
  createdByUser?: User;
  lastModifiedDateTime: string;
  publishedDateTime?: string;
  supersededDateTime?: string;
  sheetVersionId?: number;
  sheetVersionNumber?: number;
};

export type SheetVersion = {
  id: number;
  sheetId: number;
  versionNumber: number;
  publishedDateTime: string;
  publishedByUserId: number;
  publishedByUser?: User;
  changeLog?: string;
  hasBeenMigrated: boolean;
  sourceSpecSheetId?: number;
  legacySpecSheetId?: number;
  sourceCreatedByUserId?: number;
  sourceCreatedDateTime?: string;
};

export type SheetCellValue = {
  $type: string;
  sheetTableCellId: number;
  value?: string | number | null;
  valueX?: string;
  valueY?: string;
  tableReferenceId?: string;
  linkedSpecSheetTypeId?: number;
  linkedSpecSheetPhaseId?: number;
  linkedReferenceId?: string;
};

export type SheetTableCell = {
  id: number;
  sheetTableRowId: number;
  tableCellTemplateId: number;
  tableCellTypeId: number;
  cellConfigurationId: number;
  columnNumber: number;
  colSpan: number;
  rowSpan: number;
  referenceId: string;
  createdDateTime: string;
  value: SheetCellValue;
  configurationOverride?: CellProperties;
  cellConfiguration?: CellConfiguration;
  tableCellType?: TableCellType;
};

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

export type SheetWidget = {
  id: number;
  sheetTableId?: number;
  sheetTableGroupId?: number;
  tableTemplateWidgetId?: number;
  tableGroupTemplateWidgetId?: number;
  value?: WidgetValue;
};

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

export type SheetTableNote = {
  id: number;
  sheetTableId: number;
  note: string;
  isObsolete: boolean;
  sourceSystem: NoteSourceSystem;
  createdByUserId: number;
  createdDateTime: string;
  createdByUser?: User;
};

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

export type SheetProperty = {
  id: number;
  sheetId: number;
  specSheetTypePropertyId: number;
  createdDateTime: string;
  revision: SheetItemRevision;
  value?: string;
  specSheetTypeProperty?: SpecSheetTypeProperty;
};

export type SheetExternalReference = {
  id: number;
  sheetId: number;
  referenceId: string;
  externalId: number;
  systemName: string;
  ownerName: string;
  url: string;
  type: string;
  grouping?: string;
  systemIconUrl?: string;
  description?: string;
};

export type Sheet = {
  id: number;
  referenceId: string;
  specSheetPhaseId: number;
  specSheetTypeId: number;
  createdDateTime: string;
  version?: SheetVersion;
  asOf: string;
  isDraft: boolean;
  specSheetPhase?: SpecSheetPhase;
  specSheetType?: SpecSheetType;
  tables: SheetTable[];
  properties: SheetProperty[];
  externalReferences: SheetExternalReference[];
};

export type SheetVersionSummary = {
  sheetId: number;
  specSheetPhaseId: number;
  specSheetTypeId: number;
  specSheetPhase?: SpecSheetPhase;
  specSheetType?: SpecSheetType;
  version: SheetVersion;
  properties: SheetProperty[];
};

export type SheetSubmissionApprover = {
  sheetSubmissionId: number;
  userId: number;
  user?: User;
  decision: number;
  decidedDateTime?: string;
  reason?: string;
};

export type SheetSubmission = {
  id: number;
  sheetId: number;
  note?: string;
  submittedByUserId: number;
  submittedByUser?: User;
  submittedDateTime: string;
  status: number;
  decidedDateTime?: string;
  sheetVersionId?: number;
  sheetVersionNumber?: number;
  approvers: SheetSubmissionApprover[];
};

export type SheetSummary = {
  id: number;
  referenceId: string;
  specSheetPhaseId: number;
  specSheetTypeId: number;
  createdDateTime: string;
  specSheetPhase?: SpecSheetPhase;
  specSheetType?: SpecSheetType;
  latestVersion?: SheetVersion;
  draftSpecSheetId?: number;
  properties: SheetProperty[];
};

export type SheetLocation = {
  specSheetId: number;
  sheetId: number;
  versionNumber?: number;
  isDraft: boolean;
};
