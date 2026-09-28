import { Component, computed, inject, input } from '@angular/core';
import { cellKindInfo } from '../models/cell-kinds';
import { CellKindInfo } from '../models/cell-kind-info.model';
import { CellLayout } from '../models/cell-layout.model';
import { CellType } from '../models/cell-type.model';
import { GridStyle } from '../models/grid-style';
import { PanelNavigator } from '../panel-navigator';
import { TemplatesStore } from '../templates.store';

/**
 * A cell in the preview, placed on its section's subgrid by row, column and spans. Label cells show
 * their text; input cells show their caption and cell type.
 */
@Component({
  selector: 'app-canvas-cell',
  templateUrl: './canvas-cell.html',
  styleUrl: './canvas-cell.scss',
  host: {
    '[style]': 'hostStyle()',
    '[class.selected]': 'isSelected()',
    '[class.in-row]': 'isInSelectedRow()',
    '[class.label]': 'isLabel()',
  },
})
export class CanvasCell {
  public readonly layout = input.required<CellLayout>();

  public readonly cellType = computed<CellType | undefined>(() =>
    this.store.cellType(this.layout().cell.cellTypeId),
  );

  public readonly kind = computed<CellKindInfo>(() =>
    cellKindInfo(this.cellType()?.kind ?? 'Text'),
  );

  private readonly navigator = inject(PanelNavigator);
  private readonly store = inject(TemplatesStore);

  public hostStyle(): GridStyle {
    return this.layout().style;
  }

  public isLabel(): boolean {
    return this.cellType()?.kind === 'Label';
  }

  public isSelected(): boolean {
    return this.navigator.isShowing({ kind: 'cell', id: this.layout().cell.id });
  }

  public isInSelectedRow(): boolean {
    return this.navigator.isShowing({ kind: 'row', id: this.layout().rowId });
  }

  public typeName(): string {
    return this.cellType()?.name ?? 'Unknown type';
  }

  public open(): void {
    this.navigator.open({ kind: 'cell', id: this.layout().cell.id });
  }
}
