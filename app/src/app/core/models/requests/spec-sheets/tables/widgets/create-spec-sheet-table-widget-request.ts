import { TableWidgetValue } from '../../../../domain/values/table-widget-value';

export type CreateSpecSheetTableWidgetRequest = {
  specSheetTableId: number;
  tableTemplateWidgetId: number;
  value: TableWidgetValue;
};
