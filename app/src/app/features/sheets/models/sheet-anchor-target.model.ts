import { SheetAnchorKind } from './sheet-anchor-kind';

/**
 * A place on a sheet that the address can point at, written in its fragment as `#<kind>-<publicId>`.
 * The public id stays the same from one version of the sheet to the next, so a link keeps working
 * after a publish.
 */
export interface SheetAnchorTarget {
  kind: SheetAnchorKind;
  publicId: string;
}
