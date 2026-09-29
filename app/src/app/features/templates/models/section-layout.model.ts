import { CellLayout } from './cell-layout.model';
import { GridStyle } from './grid-style';
import { TemplateSection } from './template-section.model';

/** Where a section and its contents sit. Each section is an invisible subgrid of its parent. */
export interface SectionLayout {
  section: TemplateSection;
  isLeaf: boolean;
  style: GridStyle;
  /** Placement of the "empty section" filler for a leaf section without cells, otherwise null. */
  emptyStyle: GridStyle | null;
  children: SectionLayout[];
  cells: CellLayout[];
}
