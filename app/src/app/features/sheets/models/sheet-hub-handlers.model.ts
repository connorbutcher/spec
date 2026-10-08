import { RowCheckout } from './row-checkout.model';
import { RowTakeover } from './row-takeover.model';
import { SheetPresenceUser } from './sheet-presence-user.model';

/** What to do with each thing the server says over the live connection. */
export interface SheetHubHandlers {
  presenceChanged: (users: SheetPresenceUser[]) => void;
  /** Which rows people are in without having changed them. Replaces the list before. */
  checkoutsChanged: (checkouts: RowCheckout[]) => void;
  /** A row was changed or released, or a version was published. */
  sheetChanged: () => void;
  takeoverChanged: (takeover: RowTakeover) => void;
}
