import { PropertyChanged } from '../../common/property-changed';
import { TableProperties } from '../../domain/properties/table/table-properties';

export type UpdateSpecSheetTableOp = {
  $type: 'UpdateSpecSheetTableOp';
  id: number;
  title?: string;
  displayOrder?: number;
  configurationOverride?: PropertyChanged<TableProperties | undefined>;
};
