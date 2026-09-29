import { SectionRole } from './section-role';
import { TemplateRow } from './template-row.model';

/**
 * A section of a template version. It holds either child sections or rows, never both. The instance
 * counts say how many copies a sheet table starts with and may hold; a null maximum means no limit.
 */
export interface TemplateSection {
  id: number;
  parentSectionId: number | null;
  name: string;
  displayOrder: number;
  role: SectionRole;
  minInstances: number;
  maxInstances: number | null;
  initialInstances: number;
  sections: TemplateSection[];
  rows: TemplateRow[];
}
