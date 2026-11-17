import { Component, computed, inject } from '@angular/core';
import { SheetComparisonService } from '../../../services/sheet-comparison.service';
import { SheetComparisonSide } from '../../sheet-comparison-side/sheet-comparison-side';

@Component({
  selector: 'app-horizontal-sheet-compare',
  imports: [SheetComparisonSide],
  templateUrl: './horizontal-sheet-compare.html',
  styleUrl: './horizontal-sheet-compare.scss',
})
export class HorizontalSheetCompare {
  public readonly gridRows = computed(() => {
    const sheetComparison = this._sheetComparisonService.sheetComparison();
    if (!sheetComparison || sheetComparison.tableComparisons.length === 0) {
      return '1fr';
    }

    return sheetComparison.tableComparisons.map(() => 'auto').join(' ');
  });

  private readonly _sheetComparisonService = inject(SheetComparisonService);
}
