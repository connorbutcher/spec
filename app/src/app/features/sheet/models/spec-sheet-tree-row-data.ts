import { SheetListItem, SheetProperty } from '../../../core/models/domain/sheets/sheet';

export type SpecSheetTreeRowData = {
  specSheet: SheetListItem;
  properties: SheetProperty[];
};
