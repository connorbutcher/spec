/**
 * A column block of a horizontal table: people add copies of it side by side on a sheet, and each copy
 * runs through every row. Its cells are the row cells whose `columnBlockId` is this block. A null
 * `maxInstances` means no limit.
 */
export interface TemplateColumnBlock {
  id: number;
  name: string;
  displayOrder: number;
  minInstances: number;
  maxInstances: number | null;
  initialInstances: number;
  /** How many of the block's columns, from its left, stay pinned once scrolled to. */
  stickyColumnCount: number;
}
