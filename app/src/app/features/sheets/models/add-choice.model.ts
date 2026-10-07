/** One thing the add button can add where it sits: a kind of section, or a column block. */
export interface AddChoice {
  label: string;
  icon: string;
  /** Shown but not available, because the template's maximum has been reached. */
  disabled: boolean;
  add: () => void;
}
