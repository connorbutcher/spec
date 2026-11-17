import { SpecSheetPhase } from '../../../../../core/models/domain/types/spec-sheet-phase';
import { SpecSheetType } from '../../../../../core/models/domain/types/spec-sheet-type';

export type CreatePUSpecSheetDialogData = {
  type?: SpecSheetType;
  phase?: SpecSheetPhase;
  copyFromSpecSheetId?: number;
};
