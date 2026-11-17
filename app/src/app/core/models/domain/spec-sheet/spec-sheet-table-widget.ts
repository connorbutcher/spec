import { TableWidgetValue } from '../values/table-widget-value';

export type SpecSheetTableWidget = {
  id: number;
  specSheetTableId: number;
  tableTemplateWidgetId: number;
  value: TableWidgetValue;
};
