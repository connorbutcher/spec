import { GridStyle } from './grid-style';
import { TemplateColumnBlock } from './template-column-block.model';

/** Where a column block's one preview copy sits: its columns, through every row of the table. */
export interface ColumnBlockLayout {
  block: TemplateColumnBlock;
  style: GridStyle;
}
