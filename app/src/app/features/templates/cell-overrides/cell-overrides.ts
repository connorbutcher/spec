import { Component, computed, input, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { STYLE_FIELDS, CellSettingFields } from '../cell-setting-fields';
import { CellConfiguration } from '../models/cell-configuration';
import { CellKindInfo } from '../models/cell-kind-info.model';
import { CellKinds } from '../models/cell-kinds';
import { CellStyle } from '../models/cell-style.model';
import { CellType } from '../models/cell-type.model';
import { SettingField } from '../models/setting-field.model';
import { SettingValue } from '../models/setting-value';
import { TemplateCell } from '../models/template-cell.model';
import { UpdateTemplateCellOverridesRequest } from '../models/update-template-cell-overrides-request.model';
import { SettingsEditor } from '../settings-editor/settings-editor';

/**
 * The configuration and style a cell overrides from its cell type, with a button that drops every
 * override. Unmarked values are the cell type's.
 */
@Component({
  selector: 'app-cell-overrides',
  imports: [ButtonModule, SettingsEditor],
  templateUrl: './cell-overrides.html',
  styleUrl: './cell-overrides.scss',
})
export class CellOverrides {
  public readonly cell = input.required<TemplateCell>();
  public readonly cellType = input.required<CellType>();
  public readonly disabled = input(false);
  public readonly busy = input(false);

  public readonly changed = output<Partial<UpdateTemplateCellOverridesRequest>>();

  public readonly styleFields = STYLE_FIELDS;

  public readonly kind = computed<CellKindInfo>(() => CellKinds.cellKindInfo(this.cellType().kind));

  public readonly configurationFields = computed<readonly SettingField[]>(() =>
    CellSettingFields.configurationFields(this.kind().kind),
  );

  /** The cell's configuration override, if it's for its type's current kind. */
  public readonly configurationOverride = computed<CellConfiguration | null>(() => {
    const override = this.cell().configurationOverride ?? null;
    return override?.kind === this.cellType().kind ? override : null;
  });

  public readonly hasOverrides = computed(
    () => this.configurationOverride() !== null || this.cell().styleOverride != null,
  );

  public setConfigurationOverride(values: Record<string, SettingValue>): void {
    const kind = this.kind().kind;
    this.changed.emit({ configurationOverride: { ...values, kind } as CellConfiguration });
  }

  public setStyleOverride(values: Record<string, SettingValue>): void {
    this.changed.emit({ styleOverride: values as CellStyle });
  }

  public resetAll(): void {
    this.changed.emit({ configurationOverride: null, styleOverride: null });
  }
}
