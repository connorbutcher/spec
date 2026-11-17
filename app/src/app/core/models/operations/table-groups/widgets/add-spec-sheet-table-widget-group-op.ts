import { WidgetValue } from '../../../domain/values/widget-values/widget-value';

export type AddSpecSheetTableGroupWidgetOp = {
  $type: 'AddSpecSheetTableGroupWidgetOp';
  specSheetTableGroupId: number;
  tableGroupTemplateWidgetId: number;
  value: WidgetValue;
};
