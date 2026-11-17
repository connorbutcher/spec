import { CreateSpecSheetPhaseObserverRequest } from '../../access/create-spec-sheet-phase-observer-request';

export type CreateSpecSheetPhaseRequest = {
  parentSpecSheetPhaseId?: number;
  phase: string;
  phaseShort: string;
  displayOrder: number;
  specSheetTypeIds: number[];
  createSpecSheetPhaseObserverRequests: CreateSpecSheetPhaseObserverRequest[];
};
