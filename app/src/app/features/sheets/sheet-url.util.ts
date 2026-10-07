import { SheetTarget } from './models/sheet-target.model';
import { SheetView } from './models/sheet-view.model';

/** The sheet named by the route's parameters, or null when either isn't a whole number above zero. */
export function sheetTarget(
  phaseId: string | null,
  sheetTypeId: string | null,
): SheetTarget | null {
  const target = { phaseId: Number(phaseId), sheetTypeId: Number(sheetTypeId) };
  const isId = (value: number): boolean => Number.isInteger(value) && value > 0;
  return isId(target.phaseId) && isId(target.sheetTypeId) ? target : null;
}

/** The address a sheet is read from: live, as a version was published, or as it stood at a moment. */
export function sheetUrl(target: SheetTarget, view: SheetView): string {
  const url = `/api/phases/${target.phaseId}/sheets/${target.sheetTypeId}`;
  if (view.version) {
    return `${url}?version=${view.version}`;
  }
  if (view.asOf) {
    return `${url}?asOf=${encodeURIComponent(view.asOf)}`;
  }
  return url;
}
