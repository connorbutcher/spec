import { SheetLock } from './models/sheet-lock.model';

/** The lock, if someone other than the viewer holds it; null when the item is free or the viewer's own. */
export function otherUsersLock(lock: SheetLock | null | undefined): SheetLock | null {
  return lock && !lock.isMine ? lock : null;
}
