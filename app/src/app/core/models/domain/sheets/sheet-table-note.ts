import { User } from '../access/user';
import { NoteSourceSystem } from './note-source-system';

export type SheetTableNote = {
  id: number;
  sheetTableId: number;
  note: string;
  isObsolete: boolean;
  sourceSystem: NoteSourceSystem;
  createdByUserId: number;
  createdDateTime: string;
  createdByUser?: User;
};
