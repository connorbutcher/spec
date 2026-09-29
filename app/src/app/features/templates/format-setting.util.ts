import { SettingField } from './models/setting-field.model';
import { SettingValue } from './models/setting-value';

/** A setting's value as short text, for "Reset to default: ..." hints. */
export function formatSetting(field: SettingField, value: SettingValue): string {
  if (field.control === 'boolean') {
    return value === true ? 'on' : 'off';
  }
  if (value === null || value === '') {
    return 'not set';
  }
  if (field.control === 'choice') {
    return field.choices?.find((choice) => choice.value === value)?.label ?? String(value);
  }
  return String(value);
}
