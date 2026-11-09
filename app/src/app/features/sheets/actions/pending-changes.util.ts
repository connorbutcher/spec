import { SheetAnchorUtil } from '../anchors/sheet-anchor.util';
import { PendingChange } from '../models/pending-change.model';
import { PublishScope } from '../models/publish-scope';
import { SheetAnchorKind } from '../models/sheet-anchor-kind';
import { SheetLock } from '../models/sheet-lock.model';
import { SheetRow } from '../models/sheet-row.model';
import { SheetSection } from '../models/sheet-section.model';
import { Sheet } from '../models/sheet.model';
import { SheetLabelsUtil } from '../status/sheet-labels.util';

const PATH_SEPARATOR = ' › ';

export class PendingChangesUtil {
  /**
   * What a publish of `scope` would take, top to bottom: every table, section and row with a saved
   * change. Something inside an item already listed is left out, because it goes out with that item.
   */
  public static pendingChanges(sheet: Sheet | null, scope: PublishScope): PendingChange[] {
    const changes: PendingChange[] = [];
    for (const table of sheet?.tables ?? []) {
      const path = [SheetLabelsUtil.tableLabel(table)];
      if (PendingChangesUtil.isTaken(table.lock, scope)) {
        changes.push(PendingChangesUtil.change('table', table, path));
      } else {
        PendingChangesUtil.addSections(changes, table.sections, path, scope);
      }
    }
    return changes;
  }

  private static addSections(changes: PendingChange[], sections: readonly SheetSection[], parentPath: string[], scope: PublishScope): void {
    for (const section of sections) {
      const path = [...parentPath, SheetLabelsUtil.sectionLabel(section)];
      if (PendingChangesUtil.isTaken(section.lock, scope)) {
        changes.push(PendingChangesUtil.change('section', section, path));
        continue;
      }
      PendingChangesUtil.addRows(changes, section.rows, path, scope);
      PendingChangesUtil.addSections(changes, section.sections, path, scope);
    }
  }

  private static addRows(changes: PendingChange[], rows: readonly SheetRow[], parentPath: string[], scope: PublishScope): void {
    for (const row of rows) {
      if (PendingChangesUtil.isTaken(row.lock, scope)) {
        changes.push(PendingChangesUtil.change('row', row, [...parentPath, SheetLabelsUtil.rowLabel(row) ?? 'Row']));
      }
    }
  }

  /**
   * A saved change (not someone only being in the item, nor one already waiting for approval) that this
   * publish takes.
   */
  private static isTaken(lock: SheetLock | null, scope: PublishScope): boolean {
    return lock !== null && lock.isCheckout !== true && lock.isAwaitingApproval !== true && (scope === 'All' || lock.isMine);
  }

  private static change(
    kind: SheetAnchorKind,
    item: { publicId: string; isPending: boolean; lock: SheetLock | null },
    path: string[],
  ): PendingChange {
    return {
      key: SheetAnchorUtil.anchorKey(kind, item.publicId),
      path: path.join(PATH_SEPARATOR),
      kind: item.isPending ? 'New' : 'Changed',
      holder: item.lock?.isMine === false ? item.lock.userName : null,
    };
  }
}
