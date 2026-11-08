import { RowCheckout } from './row-checkout.model';
import { RowTakeover } from './row-takeover.model';
import { SheetChangedNotice } from './sheet-changed-notice.model';
import { SheetPresenceUser } from './sheet-presence-user.model';

/** What to do with each thing the server says over the live connection. */
export type SheetHubHandlers = {
  presenceChanged: (users: SheetPresenceUser[]) => void;
  /** Which rows people are in without having changed them. Replaces the list before. */
  checkoutsChanged: (checkouts: RowCheckout[]) => void;
  /** A row was checked out or released, or a version was published: the notice says which rows, if only rows. */
  sheetChanged: (notice: SheetChangedNotice | null) => void;
  takeoverChanged: (takeover: RowTakeover) => void;
}
