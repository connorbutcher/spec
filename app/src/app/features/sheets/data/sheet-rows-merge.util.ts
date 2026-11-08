import { SheetLinkableRow } from '../models/sheet-linkable-row.model';
import { SheetRow } from '../models/sheet-row.model';
import { SheetRowsChange } from '../models/sheet-rows-change.model';
import { SheetSection } from '../models/sheet-section.model';
import { Sheet } from '../models/sheet.model';

export class SheetRowsMergeUtil {
  /**
   * The open sheet with some rows as the server now has them: each row sent takes the place of the one
   * with its id, a row asked for and not sent is taken off, and the sheet-wide parts that follow from rows
   * (draft counts, linked dropdown choices, linkable rows) are taken from the change.
   *
   * Null when the change can't be put into this copy and the whole sheet has to be read: it is for
   * another sheet, a version has been published since, or it has a row this copy doesn't.
   *
   * Every table and section comes back as a new object; `reuseUnchanged` gives back the old ones of
   * everything that is equal.
   */
  public static merge(sheet: Sheet, change: SheetRowsChange, askedRowIds: readonly number[]): Sheet | null {
    if (sheet.id !== change.sheetId || !sheet.isLive || sheet.latestVersionNumber !== change.latestVersionNumber) {
      return null;
    }

    const sent = new Map(change.rows.map((row) => [row.id, row]));
    const gone = new Set(askedRowIds.filter((rowId) => !sent.has(rowId)));
    const placed = new Set<number>();
    const linkable = new Map<number, SheetLinkableRow[]>(
      change.linkableRows.map((table) => [table.sheetTableId, table.rows]),
    );
    const tables = sheet.tables.map((table) => ({
      ...table,
      sections: table.sections.map((section) => SheetRowsMergeUtil.mergeSection(section, sent, gone, placed)),
      linkableRows: linkable.get(table.id) ?? table.linkableRows,
    }));
    if (placed.size !== sent.size) {
      return null;
    }

    return {
      ...sheet,
      myDraftCount: change.myDraftCount,
      otherDrafts: change.otherDrafts,
      linkedSources: change.linkedSources,
      tables,
    };
  }

  private static mergeSection(
    section: SheetSection,
    sent: ReadonlyMap<number, SheetRow>,
    gone: ReadonlySet<number>,
    placed: Set<number>,
  ): SheetSection {
    const rows = section.rows
      .filter((row) => !gone.has(row.id))
      .map((row) => {
        const next = sent.get(row.id);
        if (next === undefined) {
          return row;
        }
        placed.add(row.id);
        return next;
      });
    return {
      ...section,
      rows,
      sections: section.sections.map((child) => SheetRowsMergeUtil.mergeSection(child, sent, gone, placed)),
    };
  }
}
