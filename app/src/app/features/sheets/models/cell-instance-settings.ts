import { LinkedDropdownInstanceSettings } from './linked-dropdown-instance-settings.model';

/**
 * The settings of one cell that are chosen on the sheet, not in the template. `kind` says which cell
 * kind they are for; it matches the API's polymorphic `CellInstanceSettings`. They are saved in the row's
 * draft and published with it, like the cell's value. A new kind of settings joins this union.
 */
export type CellInstanceSettings = LinkedDropdownInstanceSettings;
