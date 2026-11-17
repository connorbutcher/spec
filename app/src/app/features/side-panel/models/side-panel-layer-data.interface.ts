import { Signal } from '@angular/core';

export interface ISidePanelLayer<D = unknown> {
  data: D;
  title: Signal<string>;
}
