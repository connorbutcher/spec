import { SettingField } from './setting-field.model';
import { SettingValue } from './setting-value';

/** One row of the settings editor: the value in use, and whether it overrides the default. */
export interface SettingEntry {
  field: SettingField;
  value: SettingValue;
  overridden: boolean;
  defaultText: string;
}
