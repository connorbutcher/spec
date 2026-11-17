import { Component, computed } from '@angular/core';
import { PairValue } from '../../../../../../core/models/domain/values/pair-value';
import { CellComparisonBase } from '../cell-comparison-base';

@Component({
  selector: 'app-text-pair-cell-comparison',
  templateUrl: './text-pair-cell-comparison.html',
  styleUrl: './text-pair-cell-comparison.scss',
})
export class TextPairCellComparison extends CellComparisonBase {
  public override readonly cellValue = computed(() => {
    const cell = this.cell();
    if (!cell) {
      return null;
    }

    return (cell.cellValue as PairValue).valueX;
  });

  public readonly cellValueY = computed(() => {
    const cell = this.cell();
    if (!cell) {
      return null;
    }

    return (cell.cellValue as PairValue).valueY;
  });
}
