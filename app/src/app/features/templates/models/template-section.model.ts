import { TemplateRow } from './template-row.model';

/** A section of a template. It holds either child sections or rows, never both. */
export interface TemplateSection {
  id: number;
  parentSectionId: number | null;
  name: string;
  displayOrder: number;
  sections: TemplateSection[];
  rows: TemplateRow[];
}
