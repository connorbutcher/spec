import { Component, computed, inject, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CheckboxChangeEvent, CheckboxModule } from 'primeng/checkbox';
import { InputTextModule } from 'primeng/inputtext';
import { CellOverrides } from '../cell-overrides/cell-overrides';
import { CellPlacementFields } from '../cell-placement-fields/cell-placement-fields';
import { CellTypeField } from '../cell-type-field/cell-type-field';
import { CellKinds } from '../models/cell-kinds';
import { CellEntry } from '../models/cell-entry.model';
import { CellKindInfo } from '../models/cell-kind-info.model';
import { CellPlacement } from '../models/cell-placement.model';
import { CellType } from '../models/cell-type.model';
import { UpdateTemplateCellOverridesRequest } from '../models/update-template-cell-overrides-request.model';
import { UpdateTemplateCellRequest } from '../models/update-template-cell-request.model';
import { CellTypesStore } from '../cell-types.store';
import { SettingsGroup } from '../settings-group/settings-group';
import { TemplatesStore } from '../templates.store';

/**
 * Panel page for a cell: its type, caption, whether it's required, its lookup key, where it sits in the row, and
 * the configuration and style it overrides from its cell type.
 */
@Component({
  selector: 'app-cell-settings',
  imports: [
    CellOverrides,
    CellPlacementFields,
    CellTypeField,
    CheckboxModule,
    SettingsGroup,
    FormsModule,
    InputTextModule,
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
    return entry ? this.cellTypeStore.cellType(entry.cell.cellTypeId) : undefined;
  });

  public readonly kind = computed<CellKindInfo>(() =>
    CellKinds.cellKindInfo(this.cellType()?.kind ?? 'Text'),
  );

  /** The name of the column block the cell is in, or null for one of the row's own cells. */
  public readonly blockName = computed<string | null>(() => {
    const blockId = this.entry()?.cell.columnBlockId ?? null;
    return blockId === null
      ? null
      : (this.store.index().columnBlocks.get(blockId)?.block.name ?? null);
  });

  private readonly store = inject(TemplatesStore);
  private readonly cellTypeStore = inject(CellTypesStore);

  public isSaving(): boolean {
    return this.store.isSaving();
  }

  public canEdit(): boolean {
    return this.store.canEdit();
  }

  /** Whether other systems can find the cell by its text: a text cell, or any value of a labelled text cell. */
  public hasLookupKey(): boolean {
    const kind = this.kind().kind;
    return kind === 'Text' || kind === 'LabelledText';
  }

  public isDisplayOnly(): boolean {
    return CellKinds.isDisplayOnly(this.cellType()?.kind);
  }

  public setType(cellTypeId: number): void {
    void this.save({ cellTypeId });
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

  public async setLookupKey(input: HTMLInputElement): Promise<void> {
    const lookupKey = input.value.trim() || null;
    if (lookupKey !== (this.entry()?.cell.lookupKey ?? null)) {
      await this.store.updateCellLookupKey(this.cellId(), lookupKey);
    }
    input.value = this.entry()?.cell.lookupKey ?? '';
  }

  public setPlacement(changes: Partial<CellPlacement>): void {
    void this.save(changes);
  }

  public setOverrides(changes: Partial<UpdateTemplateCellOverridesRequest>): void {
    void this.store.updateCellOverrides(this.cellId(), changes);
  }

  private save(changes: Partial<UpdateTemplateCellRequest>): Promise<void> {
    return this.store.updateCell(this.cellId(), changes);
  }
}
