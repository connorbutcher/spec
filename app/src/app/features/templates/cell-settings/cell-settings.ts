import { Component, computed, inject, input } from '@angular/core';
import { ConfirmDeleteButton } from '../confirm-delete-button/confirm-delete-button';
import { cellKindInfo } from '../models/cell-kinds';
import { CellEntry } from '../models/cell-entry.model';
import { CellKindInfo } from '../models/cell-kind-info.model';
import { CellType } from '../models/cell-type.model';
import { UpdateTemplateCellRequest } from '../models/update-template-cell-request.model';
import { PanelNavigator } from '../panel-navigator';
import { TemplatesStore } from '../templates.store';

type PlacementField = 'column' | 'columnSpan' | 'rowSpan';

/** Panel page for a cell: its type, caption, whether it's required and where it sits in the row. */
@Component({
  selector: 'app-cell-settings',
  imports: [ConfirmDeleteButton],
  templateUrl: './cell-settings.html',
  styleUrl: './cell-settings.scss',
})
export class CellSettings {
  public readonly cellId = input.required<number>();

  public readonly entry = computed<CellEntry | undefined>(() =>
    this.store.index().cells.get(this.cellId()),
  );

  public readonly cellType = computed<CellType | undefined>(() => {
    const entry = this.entry();
    return entry ? this.store.cellType(entry.cell.cellTypeId) : undefined;
  });

  public readonly kind = computed<CellKindInfo>(() =>
    cellKindInfo(this.cellType()?.kind ?? 'Text'),
  );

  private readonly store = inject(TemplatesStore);
  private readonly navigator = inject(PanelNavigator);

  public cellTypes(): CellType[] {
    return this.store.cellTypes();
  }

  public isSaving(): boolean {
    return this.store.isSaving();
  }

  public isLabel(): boolean {
    return this.cellType()?.kind === 'Label';
  }

  public setType(value: string): void {
    void this.save({ cellTypeId: Number(value) });
  }

  public async setCaption(input: HTMLInputElement): Promise<void> {
    const caption = input.value.trim() || null;
    if (caption !== this.entry()?.cell.caption) {
      await this.save({ caption });
    }
    input.value = this.entry()?.cell.caption ?? '';
  }

  public setRequired(checked: boolean): void {
    void this.save({ isRequired: checked });
  }

  /** Saves a whole number of at least 1, or puts the old value back. */
  public async setPlacement(field: PlacementField, input: HTMLInputElement): Promise<void> {
    const cell = this.entry()?.cell;
    const value = Number(input.value);
    if (cell && Number.isInteger(value) && value >= 1 && value <= 100 && value !== cell[field]) {
      await this.save({ [field]: value });
    }
    input.value = String(this.entry()?.cell[field] ?? '');
  }

  public openCellType(): void {
    const cellType = this.cellType();
    if (cellType) {
      this.navigator.open({ kind: 'cellType', id: cellType.id });
    }
  }

  public openRow(): void {
    const entry = this.entry();
    if (entry) {
      this.navigator.open({ kind: 'row', id: entry.row.row.id });
    }
  }

  public async delete(): Promise<void> {
    const rowId = this.entry()?.row.row.id;
    if (rowId !== undefined && (await this.store.deleteCell(this.cellId()))) {
      this.navigator.forgetMissing({ kind: 'row', id: rowId });
    }
  }

  private save(changes: Partial<UpdateTemplateCellRequest>): Promise<void> {
    return this.store.updateCell(this.cellId(), changes);
  }
}
