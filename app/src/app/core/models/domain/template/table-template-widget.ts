import { WidgetType } from '../types/widget-type';

export type TableTemplateWidget = {
  id: number;
  tableTemplateId: number;
  widgetTypeId: string;
  widgetType: WidgetType;
};
