import { TableTemplateSummary } from './table-template-summary.model';

/** A sheet type and its tables, as one group of the template list. */
export interface TemplateListGroup {
  sheetTypeId: number;
  label: string;
  icon: string;
  items: TableTemplateSummary[];
}
