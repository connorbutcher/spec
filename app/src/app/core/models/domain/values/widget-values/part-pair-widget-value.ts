import { WidgetValue } from './widget-value';

export type PartPairWidgetValue = WidgetValue & {
  topValue?: string;
  bottomValue?: string;
};
