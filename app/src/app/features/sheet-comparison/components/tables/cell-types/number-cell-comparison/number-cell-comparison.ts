import { DecimalPipe } from '@angular/common';
import { Component } from '@angular/core';
import { CellComparisonBase } from '../cell-comparison-base';

@Component({
  selector: 'app-number-cell-comparison',
  imports: [DecimalPipe],
  templateUrl: './number-cell-comparison.html',
  styleUrl: './number-cell-comparison.scss',
})
export class NumberCellComparison extends CellComparisonBase {}
