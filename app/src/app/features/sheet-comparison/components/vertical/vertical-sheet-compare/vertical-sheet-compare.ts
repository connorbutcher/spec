import { Component, computed, inject } from '@angular/core';
import { Divider } from 'primeng/divider';
import { SheetComparisonService } from '../../../services/sheet-comparison.service';
import { SheetComparisonSide } from '../../sheet-comparison-side/sheet-comparison-side';

@Component({
  selector: 'app-vertical-sheet-compare',
  imports: [SheetComparisonSide, Divider],
  templateUrl: './vertical-sheet-compare.html',
  styleUrl: './vertical-sheet-compare.scss',
})
export class VerticalSheetCompare {
  public readonly gridRows = computed(() => {
    const sheetComparison = this._sheetComparisonService.sheetComparison();
    if (!sheetComparison || sheetComparison.tableComparisons.length === 0) {
      return '1fr';
    }

    return sheetComparison.tableComparisons.map(() => 'auto').join(' ');
  });

  private readonly _sheetComparisonService = inject(SheetComparisonService);
}
