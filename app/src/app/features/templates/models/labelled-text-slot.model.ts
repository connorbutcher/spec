/**
 * One of the values a labelled text cell holds, such as the right-hand part number. `key` ties the value
 * to the slot and never changes; `label` is shown beside it, such as "RH".
 */
export type LabelledTextSlot = {
  key: string;
  label: string;
}
