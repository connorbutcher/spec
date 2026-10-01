import { TemplateOrientation } from '../../templates/models/template-orientation';
import { AddableSection } from './addable-section.model';
import { SheetLock } from './sheet-lock.model';
import { SheetSection } from './sheet-section.model';

/** A table on a sheet, built from one version of a table template and keeping that layout. */
export interface SheetTable {
  id: number;
  publicId: string;
  tableTemplateId: number;
  templateName: string;
  templateVersionNumber: number;
  orientation: TemplateOrientation;
  title: string | null;
  displayOrder: number;
  lock: SheetLock | null;
  isPending: boolean;
  sections: SheetSection[];
  addableSections: AddableSection[];
}
