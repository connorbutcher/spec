import { TableGroupWidgetValue } from '../values/table-group-widget-value';

export type SpecSheetTableGroupWidget = {
  id: number;
  specSheetTableGroupId: number;
  tableGroupTemplateWidgetId: number;
  value: TableGroupWidgetValue;
};
