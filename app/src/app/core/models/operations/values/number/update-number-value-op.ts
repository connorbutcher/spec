import { PropertyChanged } from '../../../common/property-changed';

export type UpdateNumberValueOp = {
  $type: 'UpdateNumberValueOp';
  id: number;
  value?: PropertyChanged<number | undefined>;
};
