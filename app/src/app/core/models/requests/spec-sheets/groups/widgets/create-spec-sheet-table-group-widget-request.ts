import { TableGroupWidgetValue } from '../../../../domain/values/table-group-widget-value';

export type CreateSpecSheetTableGroupWidgetRequest = {
  specSheetTableGroupId: number;
  tableGroupTemplateWidgetId: number;
  value: TableGroupWidgetValue;
};
