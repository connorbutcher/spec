import { Component, computed, input } from '@angular/core';
import { Tooltip } from 'primeng/tooltip';
import { SheetListItem } from '../../../../../core/models/domain/sheets/sheet-list-item';

@Component({
  selector: 'app-spec-sheet-phase-display-property-control',
  templateUrl: './spec-sheet-phase-display-property-control.html',
  styleUrl: './spec-sheet-phase-display-property-control.scss',
  imports: [Tooltip],
})
export class SpecSheetPhaseDisplayPropertyControl {
  public readonly sheetSummary = input.required<SheetListItem>();
  public readonly displayValue = computed(() => {
    const sheet = this.sheetSummary();
    if (!sheet) {
      return 'N/A';
    }

    return sheet.specSheetPhase.phase;
  });
}
