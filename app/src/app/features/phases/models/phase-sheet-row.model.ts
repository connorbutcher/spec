/** One row of a sheet type's table on the phase page, ready to show. */
export type PhaseSheetRow = {
  /** The sheet's id; null for a sheet type without identifiers whose sheet hasn't been opened yet. */
  id: number | null;
  link: (string | number)[];
  /** The sheet's name: its identifier values for a type with identifiers, else the type's name. */
  name: string;
  identifierValues: string[];
  version: string;
  status: string;
  updated: string;
}
