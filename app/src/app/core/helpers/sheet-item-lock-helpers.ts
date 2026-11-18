import { Sheet } from '../models/domain/sheets/sheet';
import { SheetCheckoutSummary } from '../models/domain/sheets/sheet-checkout-summary';
import { SheetItemLock } from '../models/domain/sheets/sheet-item-lock';
import { SheetItemRevision } from '../models/domain/sheets/sheet-item-revision';

export class SheetItemLockHelpers {
  private static readonly DraftStatus = 0;

  /** Who has the item checked out, or undefined when it shows its published revision and anyone may edit it. */
  public static lockOf(
    revision: SheetItemRevision | undefined,
    currentUserId: number | undefined,
  ): SheetItemLock | undefined {
    if (!revision || revision.status !== SheetItemLockHelpers.DraftStatus) {
      return undefined;
    }

    return {
      userId: revision.createdByUserId,
      userName: revision.createdByUser?.displayName ?? 'another user',
      isMine: revision.createdByUserId === currentUserId,
      isAwaitingApproval: !!revision.sheetSubmissionId,
    };
  }

  /** Whether the current user may change the item. */
  public static isEditable(lock: SheetItemLock | undefined): boolean {
    return !lock || (lock.isMine && !lock.isAwaitingApproval);
  }

  /** A short description of the lock, to show on the item. */
  public static describe(lock: SheetItemLock): string {
    if (lock.isAwaitingApproval) {
      return lock.isMine
        ? 'Your changes are awaiting approval'
        : `${lock.userName}'s changes are awaiting approval`;
    }

    return lock.isMine
      ? 'Checked out to you'
      : `Checked out to ${lock.userName}`;
  }

  /** How many of the sheet's tables, groups and rows are checked out, by the current user and by others. */
  public static summarise(
    sheet: Sheet,
    currentUserId: number | undefined,
  ): SheetCheckoutSummary {
    const revisions: SheetItemRevision[] = [];
    for (const table of sheet.tables) {
      revisions.push(table.revision);
      for (const group of table.groups) {
        revisions.push(group.revision);
        for (const row of group.rows) {
          revisions.push(row.revision);
        }
      }
    }

    let mine = 0;
    let mineAwaitingApproval = 0;
    const others = new Map<string, number>();
    for (const revision of revisions) {
      const lock = SheetItemLockHelpers.lockOf(revision, currentUserId);
      if (!lock) {
        continue;
      }

      if (lock.isMine) {
        if (lock.isAwaitingApproval) {
          mineAwaitingApproval++;
        } else {
          mine++;
        }
      } else {
        others.set(lock.userName, (others.get(lock.userName) ?? 0) + 1);
      }
    }

    return {
      mine,
      mineAwaitingApproval,
      others: [...others.entries()].map(([userName, count]) => ({
        userName,
        count,
      })),
    };
  }
}
