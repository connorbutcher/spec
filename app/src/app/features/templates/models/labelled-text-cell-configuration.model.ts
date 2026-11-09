import { LabelledTextSlot } from './labelled-text-slot.model';

/**
 * Settings for a labelled text cell: several short texts in one cell, each with its label beside it.
 * Null means not set, or on an override, "use the default". The sheet can change the slots cell by cell.
 */
export type LabelledTextCellConfiguration = {
  kind: 'LabelledText';
  maxLength?: number | null;
  slots?: readonly LabelledTextSlot[] | null;
  /** A hex colour such as "#dc2626". */
  labelColor?: string | null;
}
