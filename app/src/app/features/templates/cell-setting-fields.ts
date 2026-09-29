import { CellKind } from './models/cell-kind';
import { SettingField } from './models/setting-field.model';

const DECIMAL_PLACES: SettingField = {
  key: 'decimalPlaces',
  label: 'Decimals',
  control: 'integer',
  min: 0,
  max: 6,
  placeholder: 'Any',
};

const UNIT: SettingField = {
  key: 'unit',
  label: 'Unit',
  control: 'text',
  maxLength: 20,
  placeholder: 'e.g. Nm, mm, °C',
};

/** The configuration settings each kind has, in the order they're shown. */
const CONFIGURATION_FIELDS: Readonly<Record<CellKind, readonly SettingField[]>> = {
  Heading: [],
  Group: [],
  Text: [
    {
      key: 'maxLength',
      label: 'Max length',
      control: 'integer',
      min: 1,
      max: 4000,
      placeholder: 'No limit',
    },
    { key: 'multiline', label: 'Multi-line', control: 'boolean' },
  ],
  Number: [
    { key: 'minValue', label: 'Min', control: 'decimal', decimals: 4, placeholder: 'None' },
    { key: 'maxValue', label: 'Max', control: 'decimal', decimals: 4, placeholder: 'None' },
    DECIMAL_PLACES,
    UNIT,
  ],
  Date: [{ key: 'includeTime', label: 'Include time', control: 'boolean' }],
  Checkbox: [],
  TextDropdown: [],
  NumberDropdown: [DECIMAL_PLACES, UNIT],
};

/** The style settings every kind has, in the order they're shown. */
export const STYLE_FIELDS: readonly SettingField[] = [
  { key: 'bold', label: 'Bold', control: 'boolean' },
  { key: 'italic', label: 'Italic', control: 'boolean' },
  {
    key: 'align',
    label: 'Align',
    control: 'choice',
    choices: [
      { label: 'Left', value: 'Left', icon: 'pi pi-align-left' },
      { label: 'Centre', value: 'Center', icon: 'pi pi-align-center' },
      { label: 'Right', value: 'Right', icon: 'pi pi-align-right' },
    ],
  },
  { key: 'textColor', label: 'Text colour', control: 'color' },
  { key: 'backgroundColor', label: 'Background', control: 'color' },
];

/** The configuration settings of a kind. Empty for kinds with none. */
export function configurationFields(kind: CellKind): readonly SettingField[] {
  return CONFIGURATION_FIELDS[kind];
}
