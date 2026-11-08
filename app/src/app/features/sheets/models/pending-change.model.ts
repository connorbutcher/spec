/** A table, section or row with a change waiting to be published, as the Publish popover lists it. */
export type PendingChange = {
  /** The item's anchor key (`row-<publicId>`), to go to it on the sheet. */
  key: string;
  /** Where it is, e.g. "Short block › Crankshaft › Main journal diameter". */
  path: string;
  /** "New" for an item never published, otherwise "Changed". */
  kind: string;
  /** Whose change it is, when it isn't the viewer's. */
  holder: string | null;
}
