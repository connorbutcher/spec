import { PanelRef } from './panel-ref';

/** One entry in a panel's list of things to drill into. */
export interface PanelLinkItem {
  ref: PanelRef;
  label: string;
  icon: string;
  /** Short trailing detail, such as a count. */
  meta?: string;
  muted?: boolean;
}
