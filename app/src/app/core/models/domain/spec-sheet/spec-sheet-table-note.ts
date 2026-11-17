import { User } from '../access/user';

export type NoteSourceSystem = 'puspecsheet' | 'pucards';

export type SpecSheetTableNote = {
  id: number;
  specSheetTableId: number;
  note: string;
  sourceSystem: NoteSourceSystem;
  isObsolete: boolean;
  createdByUserId: number;
  createdDateTime: string;
  createdByUser: User;
};
