import { TemplateOrientation } from './template-orientation';
import { TemplateSection } from './template-section.model';
import { TemplateVersionSummary } from './template-version-summary.model';

/**
 * A table template at one version, with that version's section tree. Only the latest version, while
 * no sheet uses it, is editable.
 */
export interface TableTemplate {
  id: number;
  sheetTypeId: number;
  name: string;
  displayOrder: number;
  versionId: number;
  versionNumber: number;
  orientation: TemplateOrientation;
  isEditable: boolean;
  versions: TemplateVersionSummary[];
  sections: TemplateSection[];
}
