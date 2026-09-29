import { CellConfiguration } from './models/cell-configuration';
import { CellStyle } from './models/cell-style.model';
import { SettingValue } from './models/setting-value';
import { SettingValues } from './models/setting-values';

/**
 * `defaults` with every value `overrides` sets laid on top. Null or missing override values mean
 * "use the default". Matches the API's `Apply`.
 */
export function applyOverrides<T extends object>(defaults: T, overrides: Partial<T> | null): T {
  const effective: Record<string, unknown> = { ...(defaults as Record<string, unknown>) };
  for (const [key, value] of Object.entries(overrides ?? {})) {
    if (value !== null && value !== undefined) {
      effective[key] = value;
    }
  }
  return effective as T;
}

/** The configuration a cell uses. An override for another kind is ignored, as it is by the API. */
export function effectiveConfiguration(
  defaults: CellConfiguration,
  override: CellConfiguration | null,
): CellConfiguration {
  return override?.kind === defaults.kind ? applyOverrides(defaults, override) : defaults;
}

/** The style a cell uses. */
export function effectiveStyle(defaults: CellStyle, override: CellStyle | null): CellStyle {
  return applyOverrides(defaults, override);
}

/** Whether a setting has a value, rather than being left to the default. */
export function hasSetting(values: SettingValues | null, key: string): boolean {
  const value = values?.[key];
  return value !== null && value !== undefined;
}

/** `values` with one setting changed. Null clears it, so an override goes back to the default. */
export function withSetting<T extends object>(values: T, key: string, value: SettingValue): T {
  return { ...values, [key]: value };
}
