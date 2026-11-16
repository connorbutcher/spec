import { SheetSummary } from '../core/models/sheet-summary.model';
import { SheetType } from '../core/models/sheet-type.model';

/** How a sheet is named among the phase's other sheets of its type: by its identifier values. */
export class SheetIdentityUtil {
  /** The sheet's value for an identifier, or an empty string when it has none. */
  public static valueOf(sheet: Pick<SheetSummary, 'identifiers'>, identifierId: number): string {
    return sheet.identifiers.find((value) => value.identifierId === identifierId)?.value ?? '';
  }

  /**
   * The sheet's identifier values in the sheet type's order, e.g. "ESN-100 · Build A", or "No identifiers"
   * for a sheet without any (the one a phase had before its type was given identifiers).
   */
  public static label(sheet: Pick<SheetSummary, 'identifiers'>, sheetType: Pick<SheetType, 'identifiers'>): string {
    const parts = sheetType.identifiers
      .map((identifier) => SheetIdentityUtil.valueOf(sheet, identifier.id))
      .filter((value) => value !== '');
    return parts.length === 0 ? 'No identifiers' : parts.join(' · ');
  }

  /** Where the sheet stands: awaiting approval, unpublished changes, or published or not. */
  public static status(sheet: Pick<SheetSummary, 'pendingSubmissionCount' | 'draftCount' | 'latestVersionNumber'>): string {
    if (sheet.pendingSubmissionCount > 0) {
      return 'Awaiting approval';
    }
    if (sheet.draftCount > 0) {
      return sheet.draftCount === 1 ? '1 draft' : `${sheet.draftCount} drafts`;
    }
    return sheet.latestVersionNumber === null ? 'Not published' : 'Published';
  }

  /** The route that opens a sheet. */
  public static link(sheet: Pick<SheetSummary, 'phaseId' | 'sheetTypeId' | 'id'>): (string | number)[] {
    return ['/phases', sheet.phaseId, 'sheets', sheet.sheetTypeId, sheet.id];
  }
}
