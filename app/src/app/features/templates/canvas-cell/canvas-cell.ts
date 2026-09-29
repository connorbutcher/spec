import { Component, computed, inject, input } from '@angular/core';
import { CanvasMenu } from '../canvas-menu';
import { effectiveConfiguration, effectiveStyle } from '../cell-settings.util';
import { cellStyleCss } from '../cell-style-css.util';
import { cellKindInfo, isDisplayOnly } from '../models/cell-kinds';
import { CellKindInfo } from '../models/cell-kind-info.model';
import { CellLayout } from '../models/cell-layout.model';
import { CellType } from '../models/cell-type.model';
import { GridStyle } from '../models/grid-style';
import { PanelNavigator } from '../panel-navigator';
import { TemplatesStore } from '../templates.store';

const FLEX_ALIGNMENT: Readonly<Record<string, string>> = {
  left: 'flex-start',
  center: 'center',
  right: 'flex-end',
};

/**
 * A cell in the preview, placed on its section's subgrid by row, column and spans. Label cells show
 * their text; input cells show their caption and cell type. Each cell shows its effective style (its
 * cell type's, with the cell's overrides on top). Right-click opens the quick-edit menu.
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

  /** The cell type's style with the cell's own overrides on top, as CSS. */
  public readonly styleCss = computed<Record<string, string>>(() => {
    const cellType = this.cellType();
    return cellType
      ? cellStyleCss(effectiveStyle(cellType.style, this.layout().cell.styleOverride))
      : {};
  });

  private readonly navigator = inject(PanelNavigator);
  private readonly store = inject(TemplatesStore);
  private readonly menu = inject(CanvasMenu);

  /** The grid placement plus the cell's background, which fills the whole grid area. */
  public hostStyle(): GridStyle {
    const background = this.styleCss()['background-color'];
    return background
      ? { ...this.layout().style, 'background-color': background }
      : this.layout().style;
  }

  /** The cell's text styling, with its alignment also lining up the caption and type lines. */
  public contentStyle(): Record<string, string> {
    const { 'background-color': _background, ...text } = this.styleCss();
    const align = text['text-align'];
    return align ? { ...text, 'align-items': FLEX_ALIGNMENT[align] ?? 'flex-start' } : text;
  }

  public isLabel(): boolean {
    return isDisplayOnly(this.cellType()?.kind);
  }

  /** The unit the cell uses, from its type's configuration with the cell's override on top. */
  public unit(): string | null {
    const cellType = this.cellType();
    if (!cellType) {
      return null;
    }
    const configuration = effectiveConfiguration(
      cellType.configuration,
      this.layout().cell.configurationOverride,
    );
    return 'unit' in configuration ? (configuration.unit ?? null) : null;
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

  public openMenu(event: MouseEvent): void {
    this.menu.openForCell(event, this.layout().cell.id);
  }
}
