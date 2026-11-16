import { SheetTarget } from '../models/sheet-target.model';
import { SheetView } from '../models/sheet-view.model';

export class SheetUrlUtil {
  /**
   * The sheet named by the route's parameters, or null when the phase or sheet type isn't a whole number
   * above zero, or a sheet id is given that isn't.
   */
  public static sheetTarget(
    phaseId: string | null,
    sheetTypeId: string | null,
    sheetId: string | null = null,
  ): SheetTarget | null {
    const target = {
      phaseId: Number(phaseId),
      sheetTypeId: Number(sheetTypeId),
      sheetId: sheetId === null ? null : Number(sheetId),
    };
    const validSheet = target.sheetId === null || SheetUrlUtil.isId(target.sheetId);
    return SheetUrlUtil.isId(target.phaseId) && SheetUrlUtil.isId(target.sheetTypeId) && validSheet
      ? target
      : null;
  }

  /**
   * The address a sheet is read from: live, as a version was published, as it stood at a moment, or with
   * a submission's changes on top.
   */
  public static sheetUrl(target: SheetTarget, view: SheetView): string {
    const url =
      target.sheetId === null
        ? `/api/phases/${target.phaseId}/sheets/${target.sheetTypeId}`
        : `/api/sheets/${target.sheetId}`;
    if (view.version) {
      return `${url}?version=${view.version}`;
    }
    if (view.asOf) {
      return `${url}?asOf=${encodeURIComponent(view.asOf)}`;
    }
    if (view.submission) {
      return `${url}?submission=${view.submission}`;
    }
    return url;
  }

  /** The submission named by the `review` query parameter, or null when it isn't a whole number above zero. */
  public static reviewedSubmission(review: string | null): number | null {
    const id = Number(review);
    return review !== null && SheetUrlUtil.isId(id) ? id : null;
  }

  private static isId(value: number): boolean {
    return Number.isInteger(value) && value > 0;
  }
}
