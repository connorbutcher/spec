import { RowTakeover } from './row-takeover.model';
import { SheetPresenceUser } from './sheet-presence-user.model';

/** What to do with each thing the server says over the live connection. */
export interface SheetHubHandlers {
  presenceChanged: (users: SheetPresenceUser[]) => void;
  /** A row was checked out or released, or a version was published. */
  sheetChanged: () => void;
  takeoverChanged: (takeover: RowTakeover) => void;
}
