import { WidgetValue } from '../values/widget-values/widget-value';

export type SheetWidget = {
  id: number;
  sheetTableId?: number;
  sheetTableGroupId?: number;
  tableTemplateWidgetId?: number;
  tableGroupTemplateWidgetId?: number;
  value?: WidgetValue;
};
