import { CreateSpecSheetPhaseObserverRequest } from '../../access/create-spec-sheet-phase-observer-request';
import { UpdateSpecSheetPhaseObserverRequest } from '../../access/update-spec-sheet-phase-observer-request';

export type UpdateSpecSheetPhaseRequest = {
  id: number;
  phase: string;
  phaseShort: string;
  isObsolete: boolean;
  parentSpecSheetPhaseId?: number;
  displayOrder: number;
  newSpecSheetTypeIds: number[];
  removedSpecSheetTypeIds: number[];
  createSpecSheetPhaseObserverRequests: CreateSpecSheetPhaseObserverRequest[];
  updateSpecSheetPhaseObserverRequests: UpdateSpecSheetPhaseObserverRequest[];
  removeSpecSheetPhaseObserverIds: number[];
};
