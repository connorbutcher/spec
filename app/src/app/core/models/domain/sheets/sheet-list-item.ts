import { SpecSheetPhase } from '../types/spec-sheet-phase';
import { SpecSheetType } from '../types/spec-sheet-type';
import { SheetVersion } from './sheet-version';
import { SheetProperty } from './sheet-property';

export type SheetListItem = {
  sheetId: number;
  specSheetPhaseId: number;
  specSheetTypeId: number;
  specSheetPhase?: SpecSheetPhase;
  specSheetType?: SpecSheetType;
  versionNumber: number;
  isDraft: boolean;
  version?: SheetVersion;
  properties: SheetProperty[];
};
