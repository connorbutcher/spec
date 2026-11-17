import { WidgetValue } from './widget-values/widget-value';

export type TableWidgetValue = {
  id: number;
  specSheetTableWidgetId: number;
  value?: WidgetValue;
};
