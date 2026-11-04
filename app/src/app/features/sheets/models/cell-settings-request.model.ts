import { CellInstanceSettings } from './cell-instance-settings';

/** The settings to choose on the sheet for one cell. Null clears them. */
export type CellSettingsRequest = {
  sheetCellId: number;
  settings: CellInstanceSettings | null;
}
