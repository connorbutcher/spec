import { PropertyChanged } from '../../../common/property-changed';

export type UpdateTextValueOp = {
  $type: 'UpdateTextValueOp';
  id: number;
  value?: PropertyChanged<string | undefined>;
};
