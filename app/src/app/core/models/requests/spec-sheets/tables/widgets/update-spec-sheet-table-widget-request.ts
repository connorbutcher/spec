import { PropertyChanged } from '../../../../common/property-changed';
import { WidgetValue } from '../../../../domain/values/widget-values/widget-value';

export type UpdateSpecSheetTableWidgetRequest = {
  id: number;
  widgetValue?: PropertyChanged<WidgetValue | undefined>;
};
