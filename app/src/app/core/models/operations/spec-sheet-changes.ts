import { SpecSheetOp } from './spec-sheet-op';

export type SpecSheetChanges = {
  sheetId: number;
  operations: SpecSheetOp[];
};
