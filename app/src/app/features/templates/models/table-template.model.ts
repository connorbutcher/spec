import { TemplateColumnBlock } from './template-column-block.model';
import { TemplateOrientation } from './template-orientation';
import { TemplateSection } from './template-section.model';
import { TemplateVersionSummary } from './template-version-summary.model';

/**
 * A table template at one version, with that version's section tree and column blocks. Only the
 * latest version, while no sheet uses it, is editable.
 */
export interface TableTemplate {
  id: number;
  sheetTypeId: number;
  name: string;
  displayOrder: number;
  versionId: number;
  versionNumber: number;
  orientation: TemplateOrientation;
  /** How many of the table's own columns, from the left, stay pinned while the rest scroll. */
  stickyColumnCount: number;
  isEditable: boolean;
  versions: TemplateVersionSummary[];
  sections: TemplateSection[];
  /** Left to right. Only horizontal tables have any. */
  columnBlocks: TemplateColumnBlock[];
}
