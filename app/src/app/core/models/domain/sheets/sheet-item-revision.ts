import { User } from '../access/user';

export type SheetItemRevision = {
  id: number;
  revisionNumber: number;
  status: number;
  createdByUserId: number;
  createdDateTime: string;
  createdByUser?: User;
  lastModifiedDateTime: string;
  publishedDateTime?: string;
  supersededDateTime?: string;
  sheetVersionId?: number;
  sheetVersionNumber?: number;
};
