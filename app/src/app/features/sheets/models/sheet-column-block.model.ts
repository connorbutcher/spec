import { SheetChange } from './sheet-change.model';
import { SheetLock } from './sheet-lock.model';

/**
 * A copy of a column block on a horizontal sheet table. It runs through every row of the table: a row's
 * cells for it are the cells whose `sheetColumnBlockId` is this block's id.
 */
export interface SheetColumnBlock {
  id: number;
  publicId: string;
  templateColumnBlockId: number;
  name: string;
  minInstances: number;
  maxInstances: number | null;
  initialInstances: number;
  displayOrder: number;
  lock: SheetLock | null;
  isPending: boolean;
  canRemove: boolean;
  /** The publish that last added or moved the block. */
  lastChange: SheetChange | null;
}
