export type AddSpecSheetTableRowOp = {
  $type: 'AddSpecSheetTableRowOp';
  tempId: number;
  specSheetTableGroupId: number;
  tableRowTemplateId: number;
  rowNumber: number;
  referenceId: string;
};
