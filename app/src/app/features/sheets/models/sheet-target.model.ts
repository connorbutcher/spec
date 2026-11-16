/**
 * Which sheet is open, as named in the route: a phase and sheet type, and the sheet's id when the type can
 * have many sheets in a phase. Without an id it is the phase's one sheet of the type.
 */
export type SheetTarget = {
  phaseId: number;
  sheetTypeId: number;
  sheetId: number | null;
}
