import { SheetIdentifierKind } from '../../../core/models/sheet-identifier-kind';

/** Adds or changes an identifier of a sheet type. The kind can't change once a sheet has a value for it. */
export type SaveSheetTypeIdentifierRequest = {
  name: string;
  kind: SheetIdentifierKind;
  isRequired: boolean;
  /** The choices of an option identifier, in order; ignored for the other kinds. */
  options: string[];
}
