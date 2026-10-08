/**
 * Settings for a linked dropdown cell. Its choices aren't set in the template: each cell is pointed at a
 * column of another table on its sheet (see `LinkedDropdownInstanceSettings`).
 */
export interface LinkedDropdownCellConfiguration {
  kind: 'LinkedDropdown';
}
