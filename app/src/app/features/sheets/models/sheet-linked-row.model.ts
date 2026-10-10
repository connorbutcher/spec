import { SheetLinkedValue } from './sheet-linked-value.model';

/**
 * The row behind one choice of a linked dropdown: the row of the source table whose value in the linked
 * column is `option`, with that row's other values, left to right.
 */
export interface SheetLinkedRow {
  option: string;
  sheetRowId: number;
  values: SheetLinkedValue[];
}
