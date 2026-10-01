import { SheetCell } from './sheet-cell.model';
import { SheetLock } from './sheet-lock.model';

/**
 * A row of a sheet section. A lock held by the viewer means they can edit it; a lock held by someone
 * else means it's read-only until they publish or discard. A pending row has never been published.
 */
export interface SheetRow {
  id: number;
  publicId: string;
  templateRowId: number;
  displayOrder: number;
  lock: SheetLock | null;
  isPending: boolean;
  canRemove: boolean;
  cells: SheetCell[];
}
