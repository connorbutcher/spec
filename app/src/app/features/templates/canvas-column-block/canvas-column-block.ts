import { Component, inject, input } from '@angular/core';
import { ColumnBlockLayout } from '../models/column-block-layout.model';
import { GridStyle } from '../models/grid-style';
import { PanelNavigator } from '../panel-navigator';

/**
 * The extent of a column block in the preview: a quiet dashed frame over the block's columns, through
 * every row, with the block's name in its top corner. It sits on top of the cells without catching
 * clicks, so the cells underneath stay usable; selecting the block outlines it in the accent colour.
 */
@Component({
  selector: 'app-canvas-column-block',
  templateUrl: './canvas-column-block.html',
  styleUrl: './canvas-column-block.scss',
  host: {
    '[style]': 'hostStyle()',
    '[class.selected]': 'isSelected()',
    'aria-hidden': 'true',
  },
})
export class CanvasColumnBlock {
  public readonly layout = input.required<ColumnBlockLayout>();

  private readonly navigator = inject(PanelNavigator);

  public hostStyle(): GridStyle {
    return this.layout().style;
  }

  public isSelected(): boolean {
    return this.navigator.isShowing({ kind: 'columnBlock', id: this.layout().block.id });
  }
}
