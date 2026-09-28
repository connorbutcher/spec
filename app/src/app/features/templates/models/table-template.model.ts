import { TemplateOrientation } from './template-orientation';
import { TemplateSection } from './template-section.model';

/** A table template with its whole section tree, each level in display order. */
export interface TableTemplate {
  id: number;
  sheetTypeId: number;
  name: string;
  displayOrder: number;
  orientation: TemplateOrientation;
  sections: TemplateSection[];
}
