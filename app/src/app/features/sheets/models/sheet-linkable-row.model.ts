/**
 * A row of a sheet table that a linked dropdown can take its choices from, named by the first value in it,
 * such as "Pin offset (mm)". Its other values are what the dropdown would offer.
 */
export type SheetLinkableRow = {
  sheetRowId: number;
  label: string;
}
