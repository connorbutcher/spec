import { Component, computed, inject, input } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { ConfirmDeleteButton } from '../confirm-delete-button/confirm-delete-button';
import { cellKindInfo } from '../models/cell-kinds';
import { PanelLinkItem } from '../models/panel-link-item.model';
import { RowEntry } from '../models/row-entry.model';
import { TemplateCell } from '../models/template-cell.model';
import { MoveButtons } from '../move-buttons/move-buttons';
import { PanelLinkList } from '../panel-link-list/panel-link-list';
import { PanelNavigator } from '../panel-navigator';
import { TemplatesStore } from '../templates.store';

/** Panel page for a row: where it sits in its section and its cells. */
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

  public readonly cells = computed<PanelLinkItem[]>(() =>
    (this.entry()?.row.cells ?? []).map((cell) => this.cellLink(cell)),
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

  public async addCell(): Promise<void> {
    const id = await this.store.addCell(this.rowId());
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

    return {
      ref: { kind: 'cell', id: cell.id },
      label: cell.caption || cellType?.name || 'Cell',
      icon: cellKindInfo(cellType?.kind ?? 'Text').icon,
      meta: [`Col ${cell.column}`, ...spans].join(' · '),
    };
  }
}
