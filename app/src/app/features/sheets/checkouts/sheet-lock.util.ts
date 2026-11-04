import { SheetLock } from '../models/sheet-lock.model';

export class SheetLockUtil {
  /** The lock, if someone other than the viewer holds it; null when the item is free or the viewer's own. */
  public static otherUsersLock(lock: SheetLock | null | undefined): SheetLock | null {
    return lock && !lock.isMine ? lock : null;
  }
}
