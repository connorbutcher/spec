import { SpecSheetOp } from './spec-sheet-op';

export type SheetChanges = {
  sheetId: number;
  operations: SpecSheetOp[];
};
