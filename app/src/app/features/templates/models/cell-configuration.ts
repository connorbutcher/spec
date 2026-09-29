import { CheckboxCellConfiguration } from './checkbox-cell-configuration.model';
import { DateCellConfiguration } from './date-cell-configuration.model';
import { GroupCellConfiguration } from './group-cell-configuration.model';
import { HeadingCellConfiguration } from './heading-cell-configuration.model';
import { NumberCellConfiguration } from './number-cell-configuration.model';
import { NumberDropdownCellConfiguration } from './number-dropdown-cell-configuration.model';
import { TextCellConfiguration } from './text-cell-configuration.model';
import { TextDropdownCellConfiguration } from './text-dropdown-cell-configuration.model';

/**
 * A cell type's kind-specific settings, or the part of them a cell overrides. `kind` says which; it
 * matches the API's polymorphic `CellConfiguration`.
 */
export type CellConfiguration =
  | HeadingCellConfiguration
  | GroupCellConfiguration
  | TextCellConfiguration
  | NumberCellConfiguration
  | DateCellConfiguration
  | CheckboxCellConfiguration
  | TextDropdownCellConfiguration
  | NumberDropdownCellConfiguration;
