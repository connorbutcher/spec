import { Component, computed, inject, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CheckboxChangeEvent, CheckboxModule } from 'primeng/checkbox';
import { InputTextModule } from 'primeng/inputtext';
import { SelectChangeEvent, SelectModule } from 'primeng/select';
import { ConfirmDeleteButton } from '../confirm-delete-button/confirm-delete-button';
import { cellKindInfo } from '../models/cell-kinds';
import { CellEntry } from '../models/cell-entry.model';
import { CellKindInfo } from '../models/cell-kind-info.model';
import { CellType } from '../models/cell-type.model';
import { UpdateTemplateCellRequest } from '../models/update-template-cell-request.model';
import { NumberField } from '../number-field/number-field';
import { PanelNavigator } from '../panel-navigator';
import { TemplatesStore } from '../templates.store';

type PlacementField = 'column' | 'columnSpan' | 'rowSpan';

/** Panel page for a cell: its type, caption, whether it's required and where it sits in the row. */
@Component({
  selector: 'app-cell-settings',
  imports: [
    ButtonModule,
    CheckboxModule,
    ConfirmDeleteButton,
    FormsModule,
    InputTextModule,
    NumberField,
    SelectModule,
  ],
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

  public readonly typeOptions = computed(() =>
    this.store.cellTypes().map((cellType) => ({
      label: `${cellType.name} (${cellType.kind})`,
      value: cellType.id,
      icon: cellKindInfo(cellType.kind).icon,
    })),
  );

  private readonly store = inject(TemplatesStore);
  private readonly navigator = inject(PanelNavigator);

  public isSaving(): boolean {
    return this.store.isSaving();
  }

  public canEdit(): boolean {
    return this.store.canEdit();
  }

  public isLabel(): boolean {
    return this.cellType()?.kind === 'Label';
  }

  public setType(event: SelectChangeEvent): void {
    void this.save({ cellTypeId: event.value as number });
  }

  public async setCaption(input: HTMLInputElement): Promise<void> {
    const caption = input.value.trim() || null;
    if (caption !== this.entry()?.cell.caption) {
      await this.save({ caption });
    }
    input.value = this.entry()?.cell.caption ?? '';
  }

  public setRequired(event: CheckboxChangeEvent): void {
    void this.save({ isRequired: event.checked === true });
  }

  public setPlacement(field: PlacementField, value: number | null): void {
    if (value !== null) {
      void this.save({ [field]: value });
    }
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
