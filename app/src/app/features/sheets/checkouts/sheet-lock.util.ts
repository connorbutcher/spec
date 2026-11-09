import { SheetLock } from '../models/sheet-lock.model';

export class SheetLockUtil {
  /**
   * The lock, if it keeps the viewer out: someone else holds it, or the change waits for approval, which
   * freezes it for its own author too. Null when the item is free or the viewer's own to change.
   */
  public static otherUsersLock(lock: SheetLock | null | undefined): SheetLock | null {
    return lock && (!lock.isMine || lock.isAwaitingApproval === true) ? lock : null;
  }
}
