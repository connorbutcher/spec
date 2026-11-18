import { ConnectedUser } from './connected-user';

/** Who has a sheet open. */
export type SheetPresence = {
  sheetId: number;
  users: ConnectedUser[];
};
