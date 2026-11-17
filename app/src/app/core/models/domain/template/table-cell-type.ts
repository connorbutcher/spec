import { CellTypeKey } from '../enums/cell-type-key';
import { CellTypeReferenceKey } from '../enums/cell-type-reference-key';
import { CellStyling } from '../styling/cell-styling';

export type TableCellType = {
  id: number;
  key: CellTypeReferenceKey;
  title: string;
  description: string;
  type: CellTypeKey;
  modelType: string;
  requiresUserConfiguration: boolean;
  styling: CellStyling;
};
