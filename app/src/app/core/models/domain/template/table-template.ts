import { TableProperties } from '../properties/table/table-properties';
import { TableGroupTemplate } from './table-group-template';
import { TableTemplateWidget } from './table-template-widget';

export type TableTemplate = {
  id: number;
  specSheetTypeId: number;
  type: string;
  description: string;
  requiresUserConfiguration: boolean;
  showNotes: boolean;
  isLegacy: boolean;
  defaultConfiguration: TableProperties;
  tableGroupTemplates: TableGroupTemplate[];
  tableTemplateWidgets: TableTemplateWidget[];
};
