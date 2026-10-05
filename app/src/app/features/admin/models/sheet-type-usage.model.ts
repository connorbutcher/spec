import { SheetType } from '../../../core/models/sheet-type.model';

/** A sheet type with the codes of the phases that have it. */
export interface SheetTypeUsage {
  sheetType: SheetType;
  phaseCodes: string[];
}
