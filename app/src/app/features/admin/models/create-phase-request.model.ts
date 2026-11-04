/** A new phase: under `parentPhaseId`, or at the top level when that is null. */
export type CreatePhaseRequest = {
  code: string;
  description: string | null;
  parentPhaseId: number | null;
  sheetTypeIds: number[];
}
