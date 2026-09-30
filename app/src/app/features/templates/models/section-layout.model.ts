import { CellLayout } from './cell-layout.model';
import { GridStyle } from './grid-style';
import { TemplateSection } from './template-section.model';

/** Where a section and its contents sit. Each section is an invisible subgrid of its parent. */
export interface SectionLayout {
  section: TemplateSection;
  isLeaf: boolean;
  style: GridStyle;
  /**
   * Placement of the dashed "add" area: after an addable section's sub-sections, or an empty header's
   * first row. Null for a header that already has rows.
   */
  addAreaStyle: GridStyle | null;
  children: SectionLayout[];
  cells: CellLayout[];
}
