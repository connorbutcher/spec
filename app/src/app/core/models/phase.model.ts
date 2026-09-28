/** A phase as returned by the API. The tree is built client-side from `parentPhaseId`. */
export interface Phase {
  id: number;
  code: string;
  description: string | null;
  displayOrder: number;
  parentPhaseId: number | null;
  sheetTypeIds: number[];
}
