import { PropertyChanged } from '../../../common/property-changed';
import { WidgetValue } from '../../../domain/values/widget-values/widget-value';

export type UpdateSpecSheetTableWidgetOp = {
  $type: 'UpdateSpecSheetTableWidgetOp';
  id: number;
  widgetValue?: PropertyChanged<WidgetValue | undefined>;
};
