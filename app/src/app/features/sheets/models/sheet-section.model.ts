import { SectionRole } from '../../templates/models/section-role';
import { AddableRow } from './addable-row.model';
import { AddableSection } from './addable-section.model';
import { SheetChange } from './sheet-change.model';
import { SheetLock } from './sheet-lock.model';
import { SheetRow } from './sheet-row.model';

/**
 * A section copy on a sheet table. Its own rows come first, then its sub-sections. The template's name,
 * role and instance counts are carried along so the screen can lay it out like the template.
 */
export interface SheetSection {
  id: number;
  publicId: string;
  templateSectionId: number;
  name: string;
  role: SectionRole;
  minInstances: number;
  maxInstances: number | null;
  initialInstances: number;
  displayOrder: number;
  lock: SheetLock | null;
  isPending: boolean;
  canRemove: boolean;
  rows: SheetRow[];
  sections: SheetSection[];
  addableSections: AddableSection[];
  addableRows: AddableRow[];
  /** The publish that last added, removed or moved a section or row directly in this one. */
  lastChange: SheetChange | null;
}
