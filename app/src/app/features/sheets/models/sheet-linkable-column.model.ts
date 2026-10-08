/**
 * A column of a sheet table that a linked dropdown can take its choices from: every cell of the table
 * built from the template cell `templateCellId`.
 */
export interface SheetLinkableColumn {
  templateCellId: number;
  label: string;
}
