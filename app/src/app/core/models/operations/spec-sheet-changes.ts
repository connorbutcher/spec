import { SpecSheetOp } from './spec-sheet-op';

export type SpecSheetChanges = {
  specSheetId: number;
  operations: SpecSheetOp[];
};
