import { SettingValue } from './setting-value';

/** A configuration or style object, seen by the generic settings editor as a bag of settings. */
export type SettingValues = Readonly<Record<string, SettingValue | undefined>>;
