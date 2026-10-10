import { ItemState } from './item-state';

/**
 * Everything the screen says about where an item stands: the version it is on, the one state it is in,
 * and who holds it. Worked out by `itemStatus`, and shown the same way wherever it appears.
 */
export interface ItemStatus {
  /** The publish that last changed the item, or null when it has never been published or isn't tracked. */
  versionNumber: number | null;
  /** Never published: it has no version yet. */
  isNew: boolean;
  state: ItemState;
  /** Who has the item, for every state but `Published`. */
  userName: string | null;
  isMine: boolean;
}
