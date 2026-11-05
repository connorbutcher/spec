import { PanelRef } from './panel-ref';
import { StructureLevel } from './structure-level';

/** What a node of the structure tree stands for. */
export type StructureItem = {
  /** The panel the node opens, or null for the column block group, which only groups. */
  ref: PanelRef | null;
  level: StructureLevel;
  /** A PrimeIcons class, such as `pi-folder`. */
  icon: string;
  /** A short summary shown after the label, such as "2 rows" or the cell's type. */
  meta: string;
};
