import { Component, computed, inject, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CheckboxChangeEvent, CheckboxModule } from 'primeng/checkbox';
import { InputTextModule } from 'primeng/inputtext';
import { SelectChangeEvent, SelectModule } from 'primeng/select';
import { configurationFields, STYLE_FIELDS } from '../cell-setting-fields';
import { ConfirmDeleteButton } from '../confirm-delete-button/confirm-delete-button';
import { CellConfiguration } from '../models/cell-configuration';
import { cellKindInfo, isDisplayOnly } from '../models/cell-kinds';
import { CellEntry } from '../models/cell-entry.model';
import { CellKindInfo } from '../models/cell-kind-info.model';
import { CellStyle } from '../models/cell-style.model';
import { CellType } from '../models/cell-type.model';
import { SettingField } from '../models/setting-field.model';
import { SettingValue } from '../models/setting-value';
import { UpdateTemplateCellOverridesRequest } from '../models/update-template-cell-overrides-request.model';
import { UpdateTemplateCellRequest } from '../models/update-template-cell-request.model';
import { NumberField } from '../number-field/number-field';
import { PanelNavigator } from '../panel-navigator';
import { SettingsEditor } from '../settings-editor/settings-editor';
import { TemplatesStore } from '../templates.store';

type PlacementField = 'column' | 'columnSpan' | 'rowSpan';

/**
 * Panel page for a cell: its type, caption, whether it's required, where it sits in the row, and the
 * configuration and style it overrides from its cell type.
 */
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
    SettingsEditor,
  ],
  templateUrl: './cell-settings.html',
  styleUrl: './cell-settings.scss',
})
export class CellSettings {
  public readonly cellId = input.required<number>();

  public readonly styleFields = STYLE_FIELDS;

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
      label: `${cellType.name} (${cellKindInfo(cellType.kind).label})`,
      value: cellType.id,
      icon: cellKindInfo(cellType.kind).icon,
    })),
  );

  public readonly configurationFields = computed<readonly SettingField[]>(() =>
    configurationFields(this.kind().kind),
  );

  /** The cell's configuration override, if it's for its type's current kind. */
  public readonly configurationOverride = computed<CellConfiguration | null>(() => {
    const override = this.entry()?.cell.configurationOverride ?? null;
    return override?.kind === this.cellType()?.kind ? override : null;
  });

  public readonly hasOverrides = computed(
    () => this.configurationOverride() !== null || this.entry()?.cell.styleOverride != null,
  );

  private readonly store = inject(TemplatesStore);
  private readonly navigator = inject(PanelNavigator);

  public isSaving(): boolean {
    return this.store.isSaving();
  }

  public canEdit(): boolean {
    return this.store.canEdit();
  }

  public isDisplayOnly(): boolean {
    return isDisplayOnly(this.cellType()?.kind);
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

  public setConfigurationOverride(values: Record<string, SettingValue>): void {
    const kind = this.kind().kind;
    void this.saveOverrides({
      configurationOverride: { ...values, kind } as CellConfiguration,
    });
  }

  public setStyleOverride(values: Record<string, SettingValue>): void {
    void this.saveOverrides({ styleOverride: values as CellStyle });
  }

  public resetAll(): void {
    void this.saveOverrides({ configurationOverride: null, styleOverride: null });
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

  private saveOverrides(changes: Partial<UpdateTemplateCellOverridesRequest>): Promise<void> {
    return this.store.updateCellOverrides(this.cellId(), changes);
  }
}
