import { SpecSheetPhaseObserver } from '../access/spec-sheet-phase-observer';
import { SpecSheetType } from './spec-sheet-type';

export type SpecSheetPhase = {
  id: number;
  parentSpecSheetPhaseId?: number;
  phase: string;
  phaseShort: string;
  isObsolete: boolean;
  displayOrder: number;
  phases: SpecSheetPhase[];
  types: SpecSheetType[];
  phaseObservers: SpecSheetPhaseObserver[];
};
