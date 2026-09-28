import { TemplateRow } from './template-row.model';
import { TemplateSection } from './template-section.model';

/** A row with the section it belongs to and its 1-based position there. */
export interface RowEntry {
  row: TemplateRow;
  section: TemplateSection;
  number: number;
}
