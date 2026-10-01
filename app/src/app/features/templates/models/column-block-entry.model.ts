import { TemplateColumnBlock } from './template-column-block.model';

/** A column block with its 1-based position among the table's blocks. */
export interface ColumnBlockEntry {
  block: TemplateColumnBlock;
  number: number;
}
