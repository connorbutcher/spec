import { SheetAnchorKind } from './models/sheet-anchor-kind';
import { SheetAnchorTarget } from './models/sheet-anchor-target.model';
import { SheetIndex } from './models/sheet-index.model';
import { SheetSelection } from './models/sheet-selection.model';

const KINDS: readonly SheetAnchorKind[] = ['table', 'section', 'row', 'cell'];

export class SheetAnchorUtil {
  /** `row-<publicId>`: how an anchor is written in the address's fragment, and the key it goes by on the page. */
  public static anchorKey(kind: SheetAnchorKind, publicId: string): string {
    return `${kind}-${publicId}`;
  }

  /** The anchor a fragment names, or null for no fragment or one that isn't a sheet anchor. */
  public static parseAnchor(fragment: string | null | undefined): SheetAnchorTarget | null {
    if (!fragment) {
      return null;
    }
    const dash = fragment.indexOf('-');
    const kind = KINDS.find((each) => each === fragment.slice(0, dash));
    const publicId = fragment.slice(dash + 1);
    return dash > 0 && kind !== undefined && publicId !== '' ? { kind, publicId } : null;
  }

  /** The public id of the table, section, row or cell with this id, or null when it isn't on the sheet. */
  public static anchorPublicId(index: SheetIndex, kind: SheetAnchorKind, id: number): string | null {
    return SheetAnchorUtil.itemsOf(index, kind).get(id)?.publicId ?? null;
  }

  /** The id of what an anchor points at, or null when it isn't on the sheet being viewed. */
  public static anchorId(index: SheetIndex, anchor: SheetAnchorTarget): number | null {
    for (const item of SheetAnchorUtil.itemsOf(index, anchor.kind).values()) {
      if (item.publicId === anchor.publicId) {
        return item.id;
      }
    }
    return null;
  }

  /** The row a cell is in, or null when the cell isn't on the sheet. */
  public static cellRowId(index: SheetIndex, cellId: number): number | null {
    for (const row of index.rows.values()) {
      if (row.cells.some((cell) => cell.id === cellId)) {
        return row.id;
      }
    }
    return null;
  }

  /**
   * The anchor for what is selected: the row when there is one, otherwise the section, otherwise the table.
   * Null when nothing is selected or it is no longer on the sheet.
   */
  public static selectionAnchor(index: SheetIndex, selection: SheetSelection | null): SheetAnchorTarget | null {
    if (selection === null) {
      return null;
    }
    const [kind, id]: [SheetAnchorKind, number] =
      selection.rowId !== null
        ? ['row', selection.rowId]
        : selection.sectionId !== null
          ? ['section', selection.sectionId]
          : ['table', selection.tableId];
    const publicId = SheetAnchorUtil.anchorPublicId(index, kind, id);
    return publicId === null ? null : { kind, publicId };
  }

  private static itemsOf(index: SheetIndex, kind: SheetAnchorKind): ReadonlyMap<number, { id: number; publicId: string }> {
    switch (kind) {
      case 'table': {
        return index.tables;
      }
      case 'section': {
        return index.sections;
      }
      case 'row': {
        return index.rows;
      }
      case 'cell': {
        return index.cells;
      }
    }
  }
}
