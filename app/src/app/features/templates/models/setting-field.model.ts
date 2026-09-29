import { SettingChoice } from './setting-choice.model';
import { SettingControlType } from './setting-control-type';

/** Describes one configuration or style setting, so the generic settings editor can show it. */
export interface SettingField {
  /** The property name in the configuration or style object. */
  key: string;
  label: string;
  control: SettingControlType;
  min?: number;
  max?: number;
  /** Decimal places a `decimal` setting accepts. */
  decimals?: number;
  maxLength?: number;
  placeholder?: string;
  choices?: SettingChoice[];
}
