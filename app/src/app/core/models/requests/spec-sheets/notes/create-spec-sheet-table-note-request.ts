import { NoteSourceSystem } from '../../../domain/spec-sheet/spec-sheet-table-note';

export type CreateSpecSheetTableNoteRequest = {
  specSheetTableId: number;
  note: string;
  sourceSystem: NoteSourceSystem;
  createdByUserId: number;
  createdDateTime: string;
};
