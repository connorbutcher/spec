import { SheetActionTarget } from './models/sheet-action-target.model';
import { SheetIndex } from './models/sheet-index.model';
import { SheetRow } from './models/sheet-row.model';
import { SheetSection } from './models/sheet-section.model';
import { SheetSelection } from './models/sheet-selection.model';
import { SheetTable } from './models/sheet-table.model';
import { SheetIndexUtil } from './sheet-index.util';
import { SheetLabelsUtil } from './sheet-labels.util';
import { SheetLockUtil } from './sheet-lock.util';

export class SheetActionTargetUtil {
  /**
   * What move and remove act on for a selection: the deepest thing selected that can be moved by itself.
   * A row counts only when its section has several, as a section with one row is moved and removed as a
   * whole. The header section stays where it is, so it gives no target. `tables` are the sheet's, in order.
   */
  public static sheetActionTarget(index: SheetIndex, tables: SheetTable[], selection: SheetSelection | null): SheetActionTarget | null {
    const table = selection === null ? undefined : index.tables.get(selection.tableId);
    if (selection === null || table === undefined) {
      return null;
    }
    const section = index.sections.get(selection.sectionId ?? -1);
    if (section === undefined) {
      return SheetActionTargetUtil.tableTarget(table, tables);
    }
    const row = index.rows.get(selection.rowId ?? -1);
    if (row !== undefined && row.canRemove && section.rows.length > 1) {
      return SheetActionTargetUtil.rowTarget(row, section);
    }
    return section.role === 'Header' ? null : SheetActionTargetUtil.sectionTarget(index, section);
  }

  /**
   * The nearest section, from the selected one outwards, that can take new sections: where the context
   * bar's add puts them. Null when nothing around the selection can.
   */
  public static addableSection(index: SheetIndex, selection: SheetSelection | null): SheetSection | null {
    const selected = index.sections.get(selection?.sectionId ?? -1);
    if (selected === undefined) {
      return null;
    }
    const outwards = [selected, ...SheetIndexUtil.sectionAncestors(index, selected.id).reverse()];
    return outwards.find((section) => section.addableSections.length > 0) ?? null;
  }

  private static rowTarget(row: SheetRow, section: SheetSection): SheetActionTarget {
    const isFree = SheetLockUtil.otherUsersLock(row.lock) === null;
    const position = section.rows.indexOf(row);
    return {
      kind: 'row',
      id: row.id,
      canMoveUp: isFree && position > 0,
      canMoveDown: isFree && position < section.rows.length - 1,
      canRemove: isFree,
      removeQuestion: 'Remove this row?',
    };
  }

  private static sectionTarget(index: SheetIndex, section: SheetSection): SheetActionTarget {
    const siblings = SheetIndexUtil.sectionSiblings(index, section.id);
    const first = siblings[0]?.role === 'Header' ? 1 : 0;
    const position = siblings.findIndex((candidate) => candidate.id === section.id);
    return {
      kind: 'section',
      id: section.id,
      canMoveUp: position > first,
      canMoveDown: position >= 0 && position < siblings.length - 1,
      canRemove: section.canRemove,
      removeQuestion: `Remove ${SheetLabelsUtil.sectionLabel(section)}? Its rows and sections go too.`,
    };
  }

  private static tableTarget(table: SheetTable, tables: SheetTable[]): SheetActionTarget {
    const isFree = SheetLockUtil.otherUsersLock(table.lock) === null;
    const position = tables.findIndex((candidate) => candidate.id === table.id);
    return {
      kind: 'table',
      id: table.id,
      canMoveUp: isFree && position > 0,
      canMoveDown: isFree && position >= 0 && position < tables.length - 1,
      canRemove: isFree,
      removeQuestion: `Remove ${SheetLabelsUtil.tableLabel(table)}? Everything in it goes too.`,
    };
  }
}
