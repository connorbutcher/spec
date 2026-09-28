import { CellLayout } from './cell-layout.model';
import { GridStyle } from './grid-style';
import { TemplateSection } from './template-section.model';

/** Where a section, its header and its contents sit. Each section is a subgrid of its parent. */
export interface SectionLayout {
  section: TemplateSection;
  isLeaf: boolean;
  style: GridStyle;
  headerStyle: GridStyle;
  /** Placement of the "no rows yet" filler for a leaf section without cells, otherwise null. */
  emptyStyle: GridStyle | null;
  children: SectionLayout[];
  cells: CellLayout[];
}
