import { Component } from '@angular/core';
import { SheetCompareSelection } from '../sheet-compare-selection/sheet-compare-selection';

@Component({
  selector: 'app-sheet-selection',
  imports: [SheetCompareSelection],
  templateUrl: './sheet-selection.html',
  styleUrl: './sheet-selection.scss',
})
export class SheetSelection {}
