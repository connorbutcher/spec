import { PropertyChanged } from '../../common/property-changed';

export type UpdateSpecSheetTableGroupOp = {
  $type: 'UpdateSpecSheetTableGroupOp';
  id: number;
  order?: number;
  linkedSpecSheetTableGroupId?: PropertyChanged<number | undefined>;
};
