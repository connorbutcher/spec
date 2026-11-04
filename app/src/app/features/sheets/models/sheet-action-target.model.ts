import { SheetActionKind } from './sheet-action-kind';

/**
 * The one item the context bar's move and remove act on, worked out from the selection, with what it
 * can do where it stands.
 */
export type SheetActionTarget = {
  kind: SheetActionKind;
  id: number;
  canMoveUp: boolean;
  canMoveDown: boolean;
  canRemove: boolean;
  /** What the confirm popup asks before removing it. */
  removeQuestion: string;
}
