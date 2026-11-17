import { Component, input } from '@angular/core';
import { TableComparison } from '../../../models/table-comparison';
import { VerticalSheetTableComparison } from '../../tables/vertical/vertical-sheet-table-comparison/vertical-sheet-table-comparison';

@Component({
  selector: 'app-vertical-comparison-block',
  imports: [VerticalSheetTableComparison],
  templateUrl: './vertical-comparison-block.html',
  styleUrl: './vertical-comparison-block.scss',
  host: {
    '[style.grid-row]': 'gridRow()',
  },
})
export class VerticalComparisonBlock {
  public readonly comparisonTable = input.required<TableComparison>();
  public readonly gridRow = input.required<number>();
  public readonly side = input.required<'left' | 'right'>();
}
