import { PanelRef } from './panel-ref';

/** One step of the panel's breadcrumb: where you are in the table, and the panel it opens. */
export interface PanelCrumb {
  label: string;
  ref: PanelRef;
}
