import { SpecSheetTypeProperty } from '../types/spec-sheet-type-property';
import { SheetItemRevision } from './sheet-item-revision';

export type SheetProperty = {
  id: number;
  sheetId: number;
  specSheetTypePropertyId: number;
  createdDateTime: string;
  revision: SheetItemRevision;
  value?: string;
  specSheetTypeProperty?: SpecSheetTypeProperty;
};
