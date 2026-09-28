import { Component, computed, inject, input } from '@angular/core';
import { CellTypeOptionsEditor } from '../cell-type-options-editor/cell-type-options-editor';
import { ConfirmDeleteButton } from '../confirm-delete-button/confirm-delete-button';
import { CellKind } from '../models/cell-kind';
import { CellKindInfo } from '../models/cell-kind-info.model';
import { CELL_KINDS, cellKindInfo } from '../models/cell-kinds';
import { CellType } from '../models/cell-type.model';
import { SaveCellTypeRequest } from '../models/save-cell-type-request.model';
import { PanelNavigator } from '../panel-navigator';
import { plural } from '../section-links.util';
import { TemplatesStore } from '../templates.store';

type NumberSetting = 'maxLength' | 'decimalPlaces' | 'minValue' | 'maxValue';

/** Panel page for a cell type: name, kind, description and the settings its kind uses. */
@Component({
  selector: 'app-cell-type-settings',
  imports: [CellTypeOptionsEditor, ConfirmDeleteButton],
  templateUrl: './cell-type-settings.html',
  styleUrl: './cell-type-settings.scss',
})
export class CellTypeSettings {
  public readonly cellTypeId = input.required<number>();

  public readonly kinds = CELL_KINDS;

  public readonly cellType = computed<CellType | undefined>(() =>
    this.store.cellType(this.cellTypeId()),
  );

  public readonly kind = computed<CellKindInfo>(() =>
    cellKindInfo(this.cellType()?.kind ?? 'Text'),
  );

  private readonly store = inject(TemplatesStore);
  private readonly navigator = inject(PanelNavigator);

  public isSaving(): boolean {
    return this.store.isSaving();
  }

  public usage(): string {
    const count = this.cellType()?.usageCount ?? 0;
    return count === 0 ? 'Not used by any cell yet.' : `Used by ${plural(count, 'cell')}.`;
  }

  public async rename(input: HTMLInputElement): Promise<void> {
    const name = input.value.trim();
    if (name && name !== this.cellType()?.name) {
      await this.save({ name });
    }
    input.value = this.cellType()?.name ?? '';
  }

  public setKind(value: string): void {
    if (value !== this.cellType()?.kind) {
      void this.save({ kind: value as CellKind });
    }
  }

  public async setText(
    field: 'description' | 'unit',
    input: HTMLInputElement | HTMLTextAreaElement,
  ): Promise<void> {
    const value = input.value.trim() || null;
    if (value !== this.cellType()?.[field]) {
      await this.save({ [field]: value });
    }
    input.value = this.cellType()?.[field] ?? '';
  }

  /** Saves a number setting; blank clears it. Whole-number settings ignore fractions. */
  public async setNumber(field: NumberSetting, input: HTMLInputElement): Promise<void> {
    const raw = input.value.trim();
    const value = raw === '' ? null : Number(raw);
    const wholeOnly = field === 'maxLength' || field === 'decimalPlaces';
    const valid =
      value === null || (Number.isFinite(value) && (!wholeOnly || Number.isInteger(value)));
    if (valid && value !== this.cellType()?.[field]) {
      await this.save({ [field]: value });
    }
    input.value = this.cellType()?.[field]?.toString() ?? '';
  }

  public setOptions(options: string[]): void {
    void this.save({ options });
  }

  public async delete(): Promise<void> {
    if (await this.store.deleteCellType(this.cellTypeId())) {
      this.navigator.forgetMissing({ kind: 'cellTypes' });
    }
  }

  private save(changes: Partial<SaveCellTypeRequest>): Promise<void> {
    return this.store.updateCellType(this.cellTypeId(), changes);
  }
}
