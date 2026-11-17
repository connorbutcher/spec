import { Injectable, signal } from '@angular/core';
import { SpecSheetPhase } from '../../../../core/models/domain/types/spec-sheet-phase';

@Injectable()
export class PhaseService {
  public readonly activePhase = signal<SpecSheetPhase | undefined>(undefined);
}
