import { GridStyle } from './grid-style';
import { SectionLayout } from './section-layout.model';

/** The whole table as one CSS grid, with its top-level sections as subgrids. */
export interface TemplateLayout {
  style: GridStyle;
  sections: SectionLayout[];
}
