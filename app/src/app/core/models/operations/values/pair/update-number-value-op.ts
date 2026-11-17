import { PropertyChanged } from '../../../common/property-changed';

export type UpdatePairValueOp = {
  $type: 'UpdatePairValueOp';
  id: number;
  valueX?: PropertyChanged<string | undefined>;
  valueY?: PropertyChanged<string | undefined>;
};
