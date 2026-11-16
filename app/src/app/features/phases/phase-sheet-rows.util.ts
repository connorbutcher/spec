import { Phase } from '../../core/models/phase.model';
import { SheetSummary } from '../../core/models/sheet-summary.model';
import { SheetType } from '../../core/models/sheet-type.model';
import { SheetIdentityUtil } from '../../shared/sheet-identity.util';
import { SheetLabelsUtil } from '../sheets/status/sheet-labels.util';
import { PhaseSheetRow } from './models/phase-sheet-row.model';

export class PhaseSheetRowsUtil {
  /**
   * The rows of a sheet type's table on the phase page. A type without identifiers always shows its one
   * sheet, opened by type until it exists, so opening it the first time creates it as before.
   */
  public static rows(phase: Phase, sheetType: SheetType, sheets: SheetSummary[]): PhaseSheetRow[] {
    const ofType = sheets.filter((sheet) => sheet.sheetTypeId === sheetType.id);
    if (ofType.length === 0 && sheetType.identifiers.length === 0) {
      return [
        {
          id: null,
          link: ['/phases', phase.id, 'sheets', sheetType.id],
          name: sheetType.name,
          identifierValues: [],
          version: '—',
          status: 'Not started',
          updated: '—',
        },
      ];
    }
    return ofType.map((sheet) => PhaseSheetRowsUtil.row(sheet, sheetType));
  }

  private static row(sheet: SheetSummary, sheetType: SheetType): PhaseSheetRow {
    const updated = sheet.latestPublishedAtUtc ?? sheet.createdAtUtc;
    return {
      id: sheet.id,
      link: SheetIdentityUtil.link(sheet),
      name: sheetType.identifiers.length === 0 ? sheetType.name : SheetIdentityUtil.label(sheet, sheetType),
      identifierValues: sheetType.identifiers.map((identifier) => SheetIdentityUtil.valueOf(sheet, identifier.id)),
      version: sheet.latestVersionNumber === null ? '—' : SheetLabelsUtil.versionLabel(sheet.latestVersionNumber),
      status: SheetIdentityUtil.status(sheet),
      updated: SheetLabelsUtil.momentLabel(updated),
    };
  }
}
