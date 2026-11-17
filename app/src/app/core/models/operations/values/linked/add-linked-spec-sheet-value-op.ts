export type AddLinkedSpecSheetValueOp = {
  $type: 'AddLinkedSpecSheetValueOp';
  specSheetTableCellId: number;
  linkedSpecSheetTypeId?: number;
  linkedSpecSheetPhaseId?: number;
  tableReferenceId?: string;
  linkedReferenceId?: string;
};
