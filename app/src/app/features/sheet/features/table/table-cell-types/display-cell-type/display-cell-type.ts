import { Component } from '@angular/core';
import { CellTypeBase } from '../cell-type-base';

@Component({
  selector: 'app-display-cell-type',
  templateUrl: './display-cell-type.html',
  styleUrl: './display-cell-type.scss',
  host: { class: 'cell-host' },
})
export class DisplayCellType extends CellTypeBase<string> {}
