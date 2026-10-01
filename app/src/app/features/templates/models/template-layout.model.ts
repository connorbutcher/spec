import { ColumnBlockLayout } from './column-block-layout.model';
import { GridStyle } from './grid-style';
import { SectionLayout } from './section-layout.model';

/**
 * The whole table as one CSS grid, with its top-level sections as subgrids and, for a horizontal table,
 * where each column block sits.
 */
export interface TemplateLayout {
  style: GridStyle;
  sections: SectionLayout[];
  columnBlocks: ColumnBlockLayout[];
}
