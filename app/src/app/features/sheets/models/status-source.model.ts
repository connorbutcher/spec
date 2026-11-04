import { SheetChange } from './sheet-change.model';
import { SheetLock } from './sheet-lock.model';

/** What a status is worked out from. Tables, sections, column blocks and rows all carry these. */
export type StatusSource = {
  lock: SheetLock | null;
  isPending: boolean;
  /** Left out by an item that doesn't keep one, such as a table. */
  lastChange?: SheetChange | null;
}
