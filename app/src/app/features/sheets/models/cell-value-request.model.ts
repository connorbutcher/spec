import { CellValue } from './cell-value';

/** A cell's new value, in the shape its kind takes (see `CellValue`). Null clears the cell. */
export type CellValueRequest = {
  sheetCellId: number;
  value: CellValue;
}
