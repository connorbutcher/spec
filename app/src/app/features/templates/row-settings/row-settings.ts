import { Component, computed, inject, input } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { ConfirmDeleteButton } from '../confirm-delete-button/confirm-delete-button';
import { cellKindInfo } from '../models/cell-kinds';
import { PanelLinkItem } from '../models/panel-link-item.model';
import { RowEntry } from '../models/row-entry.model';
import { TemplateCell } from '../models/template-cell.model';
import { TemplateColumnBlock } from '../models/template-column-block.model';
import { MoveButtons } from '../move-buttons/move-buttons';
import { PanelLinkList } from '../panel-link-list/panel-link-list';
import { PanelNavigator } from '../panel-navigator';
import { TemplatesStore } from '../templates.store';

/**
 * Panel page for a row: where it sits in its section and its cells, the row's own first and then, in a
 * horizontal table, its cells in each column block.
 */
@Component({
  selector: 'app-row-settings',
  imports: [ButtonModule, ConfirmDeleteButton, MoveButtons, PanelLinkList],
  templateUrl: './row-settings.html',
  styleUrl: './row-settings.scss',
})
export class RowSettings {
  public readonly rowId = input.required<number>();

  public readonly entry = computed<RowEntry | undefined>(() =>
    this.store.index().rows.get(this.rowId()),
  );

  public readonly cells = computed<PanelLinkItem[]>(() => {
    const order = new Map(this.columnBlocks().map((block, index) => [block.id, index + 1]));
    return [...(this.entry()?.row.cells ?? [])]
      .sort(
        (a, b) =>
          (a.columnBlockId === null ? 0 : (order.get(a.columnBlockId) ?? 0)) -
            (b.columnBlockId === null ? 0 : (order.get(b.columnBlockId) ?? 0)) ||
          a.column - b.column,
      )
      .map((cell) => this.cellLink(cell));
  });

  /** The table's column blocks, left to right; each can take cells in this row. */
  public readonly columnBlocks = computed<TemplateColumnBlock[]>(() =>
    [...this.store.index().columnBlocks.values()]
      .sort((a, b) => a.number - b.number)
      .map((entry) => entry.block),
  );

  private readonly store = inject(TemplatesStore);
  private readonly navigator = inject(PanelNavigator);

  public isSaving(): boolean {
    return this.store.isSaving();
  }

  public canEdit(): boolean {
    return this.store.canEdit();
  }

  public move(position: number): void {
    void this.store.moveRow(this.rowId(), position);
  }

  /** Adds a cell after the last of the row's own cells, or of a column block's cells with its id. */
  public async addCell(columnBlockId: number | null = null): Promise<void> {
    const id = await this.store.addCell(this.rowId(), null, columnBlockId);
    if (id !== null) {
      this.navigator.open({ kind: 'cell', id });
    }
  }

  public async delete(): Promise<void> {
    const sectionId = this.entry()?.section.id;
    if (sectionId !== undefined && (await this.store.deleteRow(this.rowId()))) {
      this.navigator.forgetMissing({ kind: 'section', id: sectionId });
    }
  }

  private cellLink(cell: TemplateCell): PanelLinkItem {
    const cellType = this.store.cellType(cell.cellTypeId);
    const spans = [
      cell.columnSpan > 1 ? `${cell.columnSpan} cols` : '',
      cell.rowSpan > 1 ? `${cell.rowSpan} rows` : '',
    ].filter(Boolean);

    const block =
      cell.columnBlockId === null
        ? undefined
        : this.store.index().columnBlocks.get(cell.columnBlockId);
    const place = block ? `${block.block.name} col ${cell.column}` : `Col ${cell.column}`;

    return {
      ref: { kind: 'cell', id: cell.id },
      label: cell.caption || cellType?.name || 'Cell',
      icon: cellKindInfo(cellType?.kind ?? 'Text').icon,
      meta: [place, ...spans].join(' · '),
    };
  }
}
