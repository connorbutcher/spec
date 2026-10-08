import { RowCheckout } from './row-checkout.model';
import { RowTakeover } from './row-takeover.model';
import { SheetPresenceUser } from './sheet-presence-user.model';

/**
 * What the server sends on joining a sheet: who has it open, the rows people are in without having
 * changed them, and the open takeover requests that involve the viewer.
 */
export interface SheetLiveState {
  users: SheetPresenceUser[];
  checkouts: RowCheckout[];
  takeovers: RowTakeover[];
}
