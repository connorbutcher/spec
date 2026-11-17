import { TableGroupTemplateWidget } from './table-group-template-widget';
import { TableRowTemplate } from './table-row-template';

export type TableGroupType = 'header' | 'group' | 'row' | 'column';

export type TableGroupTemplate = {
  id: number;
  tableTemplateId: number;
  parentTableGroupTemplateId?: number;
  title: string;
  groupType: TableGroupType;
  groupKey?: string;
  displayOrder: number;
  isDefault: boolean;
  copyFromPrevious: boolean;
  canUserAddToTable: boolean;
  showInContextMenu: boolean;
  canBeReordered: boolean;
  showContextMenu: boolean;
  rowTemplates: TableRowTemplate[];
  tableGroupTemplateWidgets: TableGroupTemplateWidget[];
};
