import { ConnectedUser } from './connected-user';

/** Another user saved, undid, submitted or decided changes on a sheet, so its checkouts should be read again. */
export type SheetChangedNotice = {
  sheetId: number;
  user: ConnectedUser;
  change: string;
};
