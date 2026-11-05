import { PanelRef } from './panel-ref';
import { StructureLevel } from './structure-level';

/** One step of the selection breadcrumb: where you are in the table, and the panel it opens. */
export type PanelCrumb = {
  label: string;
  ref: PanelRef;
  level: StructureLevel;
};
