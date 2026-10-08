/**
 * What a linked dropdown cell can offer right now. `unset`: nobody has chosen where its choices come
 * from. `missing`: the table or column it was pointed at is no longer on the sheet. `ready`: it has a
 * column, named by `source`, whose values are `options`.
 */
export interface LinkedChoices {
  status: 'unset' | 'missing' | 'ready';
  /** "Piston parts › Part number", or empty when there is no column to name. */
  source: string;
  options: readonly string[];
}
