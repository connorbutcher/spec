import { TemplateOrientation } from './template-orientation';

/** A table template without its sections, for the list. Orientation is the latest version's. */
export interface TableTemplateSummary {
  id: number;
  sheetTypeId: number;
  name: string;
  displayOrder: number;
  latestVersionNumber: number;
  orientation: TemplateOrientation;
}
