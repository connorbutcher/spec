/** A new phase: under `parentPhaseId`, or at the top level when that is null. */
export interface CreatePhaseRequest {
  code: string;
  description: string | null;
  parentPhaseId: number | null;
  sheetTypeIds: number[];
}
