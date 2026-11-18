import { SpecSheetPhase } from '../types/spec-sheet-phase';
import { SpecSheetType } from '../types/spec-sheet-type';
import { SheetVersion } from './sheet-version';
import { SheetTable } from './sheet-table';
import { SheetProperty } from './sheet-property';
import { SheetExternalReference } from './sheet-external-reference';

/**
 * A sheet in the shape the sheets endpoints return it: every item has its own id and the revision of it that is shown.
 */
export type Sheet = {
  id: number;
  referenceId: string;
  specSheetPhaseId: number;
  specSheetTypeId: number;
  createdDateTime: string;
  version?: SheetVersion;
  asOf: string;
  isDraft: boolean;
  specSheetPhase?: SpecSheetPhase;
  specSheetType?: SpecSheetType;
  tables: SheetTable[];
  properties: SheetProperty[];
  externalReferences: SheetExternalReference[];
};
