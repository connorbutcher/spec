import { Component, input } from '@angular/core';
import { Divider } from 'primeng/divider';
import { TableComparison } from '../../../models/table-comparison';
import { HorizontalSheetTableComparison } from '../../tables/horizontal/horizontal-sheet-table-comparison/horizontal-sheet-table-comparison';

@Component({
  selector: 'app-horizontal-comparison-block',
  imports: [Divider, HorizontalSheetTableComparison],
  templateUrl: './horizontal-comparison-block.html',
  styleUrl: './horizontal-comparison-block.scss',
  host: {
    '[style.grid-row]': 'gridRow()',
  },
})
export class HorizontalComparisonBlock {
  public readonly comparisonTable = input.required<TableComparison>();
  public readonly gridRow = input.required<number>();
  public readonly side = input.required<'left' | 'right'>();
}
