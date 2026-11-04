/**
 * Something on the sheet that a search can find: a table's heading or one cell. `key` is its anchor
 * key (`cell-<publicId>`, see `SheetAnchorService`); `text` is what it shows, in lower case.
 */
export type SheetSearchTarget = {
  key: string;
  text: string;
}
