/** One entry in the version picker. A null value is the live sheet. */
export type SheetVersionOption = {
  label: string;
  value: number | null;
  /** The note its publisher wrote, shown under the entry in the list. */
  note: string | null;
}
