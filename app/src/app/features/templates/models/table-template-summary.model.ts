import { TemplateOrientation } from './template-orientation';

/** A table template without its sections, for the list. */
export interface TableTemplateSummary {
  id: number;
  sheetTypeId: number;
  name: string;
  displayOrder: number;
  orientation: TemplateOrientation;
}
