import { Component, computed, input, output } from '@angular/core';
import { hasSetting, withSetting } from '../cell-settings.util';
import { SettingField } from '../models/setting-field.model';
import { SettingValue } from '../models/setting-value';
import { SettingValues } from '../models/setting-values';
import { SettingRow } from '../setting-row/setting-row';
import { SettingEntry } from '../models/setting-entry.model';
import { formatSetting } from '../format-setting.util';

/**
 * Edits a configuration or style object, one row per field. Without `overrides` it edits `defaults`
 * directly (a cell type). With `overridable` it shows `defaults` with `overrides` on top, and edits
 * only the overrides (a cell); each overridden row can go back to its default.
 *
 * Emits the whole new object each time: the defaults, or the overrides.
 */
@Component({
  selector: 'app-settings-editor',
  imports: [SettingRow],
  templateUrl: './settings-editor.html',
  styleUrl: './settings-editor.scss',
})
export class SettingsEditor {
  public readonly fields = input.required<readonly SettingField[]>();
  public readonly defaults = input.required<object>();
  public readonly overrides = input<object | null>(null);
  public readonly overridable = input(false);
  /** Makes the control ids unique on the page, e.g. "cell-style". */
  public readonly idPrefix = input.required<string>();
  public readonly disabled = input(false);

  public readonly changed = output<Record<string, SettingValue>>();

  public readonly entries = computed<SettingEntry[]>(() => {
    const defaults = this.defaults() as SettingValues;
    const overrides = this.overridable() ? (this.overrides() as SettingValues | null) : null;
    return this.fields().map((field) => {
      const overridden = hasSetting(overrides, field.key);
      return {
        field,
        value: (overridden ? overrides?.[field.key] : defaults[field.key]) ?? null,
        overridden,
        defaultText: formatSetting(field, defaults[field.key] ?? null),
      };
    });
  });

  public set(field: SettingField, value: SettingValue): void {
    this.changed.emit(withSetting(this.target(), field.key, value));
  }

  public reset(field: SettingField): void {
    this.changed.emit(withSetting(this.target(), field.key, null));
  }

  /** What an edit changes: the overrides on a cell, else the defaults. */
  private target(): Record<string, SettingValue> {
    const values = this.overridable() ? (this.overrides() ?? {}) : this.defaults();
    return values as Record<string, SettingValue>;
  }
}
