import { SheetTarget } from './models/sheet-target.model';
import { SheetView } from './models/sheet-view.model';

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
