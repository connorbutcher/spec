import { Signal, Type } from '@angular/core';
import { ISidePanelLayer } from './side-panel-layer-data.interface';

export interface ISidePanelLayerItem<C extends ISidePanelLayer<D>, D = unknown> {
  id: string;
  component: Type<C>;
  data: D;
  title?: Signal<string>;
}
