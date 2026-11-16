import { SheetIdentifierKind } from './sheet-identifier-kind';

/**
 * A value that tells one sheet of a sheet type from another within a phase, such as an engine
 * specification's serial number. `key` names it in the published API's filters and never changes.
 */
export type SheetTypeIdentifier = {
  id: number;
  key: string;
  name: string;
  kind: SheetIdentifierKind;
  isRequired: boolean;
  displayOrder: number;
  /** The choices of an option identifier; empty for the other kinds. */
  options: string[];
}
