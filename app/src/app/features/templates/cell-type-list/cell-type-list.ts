import { Component, computed, inject } from '@angular/core';
import { cellKindInfo } from '../models/cell-kinds';
import { PanelLinkItem } from '../models/panel-link-item.model';
import { PanelLinkList } from '../panel-link-list/panel-link-list';
import { PanelNavigator } from '../panel-navigator';
import { plural } from '../section-links.util';
import { TemplatesStore } from '../templates.store';

/** Panel page listing every cell type, with a way to add one. */
@Component({
  selector: 'app-cell-type-list',
  imports: [PanelLinkList],
  templateUrl: './cell-type-list.html',
  styleUrl: './cell-type-list.scss',
})
export class CellTypeList {
  public readonly items = computed<PanelLinkItem[]>(() =>
    this.store.cellTypes().map((cellType) => ({
      ref: { kind: 'cellType', id: cellType.id },
      label: cellType.name,
      icon: cellKindInfo(cellType.kind).icon,
      meta: cellType.usageCount > 0 ? plural(cellType.usageCount, 'cell') : cellType.kind,
    })),
  );

  private readonly store = inject(TemplatesStore);
  private readonly navigator = inject(PanelNavigator);

  public isSaving(): boolean {
    return this.store.isSaving();
  }

  public async add(): Promise<void> {
    const id = await this.store.createCellType();
    if (id !== null) {
      this.navigator.open({ kind: 'cellType', id });
    }
  }
}
