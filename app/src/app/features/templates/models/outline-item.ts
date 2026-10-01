import { TemplateColumnBlock } from './template-column-block.model';
import { TemplateSection } from './template-section.model';

/** What a node of the structure outline stands for, besides the table itself and the block group. */
export type OutlineItem =
  | { kind: 'section'; section: TemplateSection }
  | { kind: 'columnBlock'; block: TemplateColumnBlock };
