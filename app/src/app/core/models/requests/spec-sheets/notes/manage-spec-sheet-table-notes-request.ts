import { CreateSpecSheetTableNoteRequest } from './create-spec-sheet-table-note-request';
import { UpdateSpecSheetTableNoteRequest } from './update-spec-sheet-table-note-request';

export type ManageSpecSheetTableNotesRequest = {
  createRequests: CreateSpecSheetTableNoteRequest[];
  updateRequests: UpdateSpecSheetTableNoteRequest[];
};
