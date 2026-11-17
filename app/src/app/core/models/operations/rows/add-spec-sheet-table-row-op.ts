export type AddSpecSheetTableGroupOp = {
  $type: 'AddSpecSheetTableGroupOp';
  tempId: number;
  specSheetTableId: number;
  tableGroupTemplateId: number;
  linkedSpecSheetTableGroupId?: number;
  order: number;
  referenceId: string;
};
