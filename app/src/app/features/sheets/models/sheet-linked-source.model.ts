import { SheetLinkedRow } from './sheet-linked-row.model';

/**
 * The choices one column gives the linked dropdowns pointed at it: the values it holds, top to bottom,
 * without repeats. Only columns some cell is linked to are sent.
 */
export interface SheetLinkedSource {
  sheetTableId: number;
  templateCellId: number;
  options: string[];
  /** The row behind each choice, when a cell linked to this column also shows its choice's row; empty otherwise. */
  rows: SheetLinkedRow[];
}
