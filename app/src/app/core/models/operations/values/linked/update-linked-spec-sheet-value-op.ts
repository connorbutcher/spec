import { PropertyChanged } from '../../../common/property-changed';

export type UpdateLinkedSpecSheetValueOp = {
  $type: 'UpdateLinkedSpecSheetValueOp';
  id: number;
  linkedSpecSheetTypeId?: PropertyChanged<number | undefined>;
  linkedSpecSheetPhaseId?: PropertyChanged<number | undefined>;
  tableReferenceId?: PropertyChanged<string | undefined>;
  linkedReferenceId?: PropertyChanged<string | undefined>;
};
