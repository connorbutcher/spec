import { WidgetValue } from '../../../domain/values/widget-values/widget-value';

export type AddSpecSheetTableWidgetOp = {
  $type: 'AddSpecSheetTableWidgetOp';
  specSheetTableId: number;
  tableTemplateWidgetId: number;
  value: WidgetValue;
};
