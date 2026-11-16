import { SheetTypeIdentifier } from './sheet-type-identifier.model';

/** A kind of PU Spec Sheet, such as Specification or Torque Sheet. */
export type SheetType = {
  id: number;
  name: string;
  displayOrder: number;
  /** What tells the sheets of this type apart within a phase. Empty when a phase has one sheet of the type. */
  identifiers: SheetTypeIdentifier[];
}
