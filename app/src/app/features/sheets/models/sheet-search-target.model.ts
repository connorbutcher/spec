/**
 * Something on the sheet that a search can find: a table's heading or one cell. `key` is what the
 * element showing it carries in `data-search-key`; `text` is what it shows, in lower case.
 */
export interface SheetSearchTarget {
  key: string;
  text: string;
}
