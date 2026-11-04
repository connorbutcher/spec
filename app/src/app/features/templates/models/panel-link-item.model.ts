import { PanelRef } from './panel-ref';

export type PanelLinkItem = {
  ref: PanelRef;
  label: string;
  icon: string;
  /** Short trailing detail, such as a count. */
  meta?: string;
  muted?: boolean;
}
