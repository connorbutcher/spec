import { WidgetTypeKey } from '../enums/widget-type-key';

export type WidgetType = {
  id: number;
  key: WidgetTypeKey;
  title: string;
  description: string;
  modelType: string;
};
