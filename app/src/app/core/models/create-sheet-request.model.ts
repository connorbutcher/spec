import { SheetIdentifierValue } from './sheet-identifier-value.model';

/** A new sheet of a sheet type in a phase: its identifier values, and the sheet of the type to start as a copy of. */
export type CreateSheetRequest = {
  sheetTypeId: number;
  identifiers: SheetIdentifierValue[];
  copyFromSheetId: number | null;
}
