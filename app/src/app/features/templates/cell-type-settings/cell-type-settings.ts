import { Component, computed, inject, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { SelectChangeEvent, SelectModule } from 'primeng/select';
import { TextareaModule } from 'primeng/textarea';
import { configurationFields, STYLE_FIELDS } from '../cell-setting-fields';
import { CellTypeOptionsEditor } from '../cell-type-options-editor/cell-type-options-editor';
import { ConfirmDeleteButton } from '../confirm-delete-button/confirm-delete-button';
import { CellConfiguration } from '../models/cell-configuration';
import { CellKind } from '../models/cell-kind';
import { CellKindInfo } from '../models/cell-kind-info.model';
import { CELL_KINDS, cellKindInfo, isDropdown } from '../models/cell-kinds';
import { CellStyle } from '../models/cell-style.model';
import { CellType } from '../models/cell-type.model';
import { SaveCellTypeRequest } from '../models/save-cell-type-request.model';
import { SettingField } from '../models/setting-field.model';
import { SettingValue } from '../models/setting-value';
import { PanelNavigator } from '../panel-navigator';
import { plural } from '../section-links.util';
import { SettingsEditor } from '../settings-editor/settings-editor';
import { TemplatesStore } from '../templates.store';

/**
 * Panel page for a cell type: name, kind, description, and the default configuration, options and
 * style its cells start with.
 */
@Component({
  selector: 'app-cell-type-settings',
  imports: [
    CellTypeOptionsEditor,
    ConfirmDeleteButton,
    FormsModule,
    InputTextModule,
    SelectModule,
    SettingsEditor,
    TextareaModule,
  ],
  templateUrl: './cell-type-settings.html',
  styleUrl: './cell-type-settings.scss',
})
export class CellTypeSettings {
  public readonly cellTypeId = input.required<number>();

  public readonly kinds = [...CELL_KINDS];
  public readonly styleFields = STYLE_FIELDS;

  public readonly cellType = computed<CellType | undefined>(() =>
    this.store.cellType(this.cellTypeId()),
  );

  public readonly kind = computed<CellKindInfo>(() =>
    cellKindInfo(this.cellType()?.kind ?? 'Text'),
  );

  public readonly configurationFields = computed<readonly SettingField[]>(() =>
    configurationFields(this.kind().kind),
  );

  public readonly hasOptions = computed(() => isDropdown(this.cellType()?.kind));

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

  public setKind(event: SelectChangeEvent): void {
    const kind = event.value as CellKind;
    if (kind !== this.cellType()?.kind) {
      void this.save({ kind });
    }
  }

  public async setText(field: 'description', input: HTMLTextAreaElement): Promise<void> {
    const value = input.value.trim() || null;
    if (value !== this.cellType()?.[field]) {
      await this.save({ [field]: value });
    }
    input.value = this.cellType()?.[field] ?? '';
  }

  public setConfiguration(values: Record<string, SettingValue>): void {
    const kind = this.kind().kind;
    void this.save({ configuration: { ...values, kind } as CellConfiguration });
  }

  public setStyle(values: Record<string, SettingValue>): void {
    void this.save({ style: values as CellStyle });
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
