/**
 * What the server says changed on the open sheet. When only rows were checked out or released it names
 * them, and only those are read again; with null the whole sheet is.
 */
export type SheetChangedNotice = {
  rowIds: number[] | null;
}
