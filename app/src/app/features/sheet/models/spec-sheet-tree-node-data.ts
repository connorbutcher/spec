import { SheetListItem } from '../../../core/models/domain/sheets/sheet-list-item';
import { SheetProperty } from '../../../core/models/domain/sheets/sheet-property';

export type SpecSheetTreeNodeData = {
  property: SheetProperty;
  specSheets: SheetListItem[];
};
