import { User } from '../access/user';

export type SheetVersion = {
  id: number;
  sheetId: number;
  versionNumber: number;
  publishedDateTime: string;
  publishedByUserId: number;
  publishedByUser?: User;
  changeLog?: string;
  hasBeenMigrated: boolean;
  sourceSpecSheetId?: number;
  sourceCreatedByUserId?: number;
  sourceCreatedDateTime?: string;
};
