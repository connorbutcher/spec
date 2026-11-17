import { Injectable, signal } from '@angular/core';
import { SheetFilterService } from '../../sheet/services/sheet-filter.service';
import { SpecSheetState, states } from '../models/spec-sheet-state';

@Injectable()
export class SheetComparisonFilterService extends SheetFilterService {
  public readonly selectedStates = signal<SpecSheetState[]>([...states]);

  constructor() {
    super();
  }
}
