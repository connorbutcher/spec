import { Component, computed, inject } from '@angular/core';
import { SheetComparisonService } from '../../services/sheet-comparison.service';
import { HorizontalSheetCompare } from '../horizontal/horizontal-sheet-compare/horizontal-sheet-compare';
import { SheetSelection } from '../sheet-selection/sheet-selection';
import { VerticalSheetCompare } from '../vertical/vertical-sheet-compare/vertical-sheet-compare';

@Component({
  selector: 'app-sheet-compare',
  imports: [SheetSelection, VerticalSheetCompare, HorizontalSheetCompare],
  templateUrl: './sheet-compare.html',
  styleUrl: './sheet-compare.scss',
})
export class SheetCompare {
  public readonly gridRows = computed(() => {
    const sheetComparison = this._sheetComparisonService.sheetComparison();
    if (!sheetComparison || sheetComparison.tableComparisons.length === 0) {
      return '1fr';
    }

    return sheetComparison.tableComparisons.map(() => 'auto').join(' ');
  });
  public readonly tableOrientation = computed(() => {
    const specSheetType = this._sheetComparisonService.specSheetType();
    return specSheetType?.tableOrientation || 'vertical';
  });

  private readonly _sheetComparisonService = inject(SheetComparisonService);
}
