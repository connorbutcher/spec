import { Component, computed, input, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { STYLE_FIELDS, CellSettingFields } from '../cell-setting-fields';
import { CellConfiguration } from '../models/cell-configuration';
import { CellKindInfo } from '../models/cell-kind-info.model';
import { CellKinds } from '../models/cell-kinds';
import { CellStyle } from '../models/cell-style.model';
import { CellType } from '../models/cell-type.model';
import { LabelledTextSlot } from '../models/labelled-text-slot.model';
import { LabelledTextSlotsField } from '../labelled-text-slots-field/labelled-text-slots-field';
import { LabelledTextSlotsUtil } from '../labelled-text-slots.util';
import { SettingField } from '../models/setting-field.model';
import { SettingValue } from '../models/setting-value';
import { TemplateCell } from '../models/template-cell.model';
import { UpdateTemplateCellOverridesRequest } from '../models/update-template-cell-overrides-request.model';
import { SettingsEditor } from '../settings-editor/settings-editor';
import { SettingsGroup } from '../settings-group/settings-group';

/**
 * The configuration and style a cell overrides from its cell type, with a button that drops every
 * override. Unmarked values are the cell type's.
 */
@Component({
  selector: 'app-cell-overrides',
  imports: [ButtonModule, LabelledTextSlotsField, SettingsEditor, SettingsGroup],
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

  /** Whether the cell names its own labelled text values rather than its type's. */
  public readonly slotsOverridden = computed(() => {
    const override = this.configurationOverride();
    return override?.kind === 'LabelledText' && (override.slots ?? null) !== null;
  });

  /** The labelled text values the cell has: its own, or its type's. */
  public readonly slots = computed<readonly LabelledTextSlot[]>(() => {
    const override = this.configurationOverride();
    const own = override?.kind === 'LabelledText' ? (override.slots ?? null) : null;
    return own ?? LabelledTextSlotsUtil.templateSlots(this.cellType().configuration);
  });

  public readonly hasOverrides = computed(
    () => this.configurationOverride() !== null || this.cell().styleOverride != null,
  );

  /** "2 changed" for a group with overrides, or null. */
  public readonly configurationNote = computed<string | null>(() =>
    CellOverrides.changedNote(this.configurationOverride(), ['kind']),
  );

  public readonly styleNote = computed<string | null>(() =>
    CellOverrides.changedNote(this.cell().styleOverride ?? null, []),
  );

  private static changedNote(override: object | null, ignored: string[]): string | null {
    const count = Object.entries(override ?? {}).filter(
      ([key, value]) => !ignored.includes(key) && value !== null && value !== undefined,
    ).length;
    return count > 0 ? `${count} changed` : null;
  }

  public setConfigurationOverride(values: Record<string, SettingValue>): void {
    const kind = this.kind().kind;
    this.changed.emit({ configurationOverride: { ...values, kind } as CellConfiguration });
  }

  /** Gives the cell its own labelled text values, or with null goes back to its type's. */
  public setSlots(slots: LabelledTextSlot[] | null): void {
    const override = this.configurationOverride() ?? { kind: 'LabelledText' };
    this.changed.emit({ configurationOverride: { ...override, kind: 'LabelledText', slots } });
  }

  public setStyleOverride(values: Record<string, SettingValue>): void {
    this.changed.emit({ styleOverride: values as CellStyle });
  }

  public resetAll(): void {
    this.changed.emit({ configurationOverride: null, styleOverride: null });
  }
}
