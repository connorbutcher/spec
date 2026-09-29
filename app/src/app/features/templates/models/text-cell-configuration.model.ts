/** Settings for a free text cell. Null means not set, or on an override, "use the default". */
export interface TextCellConfiguration {
  kind: 'Text';
  maxLength?: number | null;
  multiline?: boolean | null;
}
