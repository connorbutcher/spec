export type SheetCellValue = {
  $type: string;
  sheetTableCellId: number;
  value?: string | number | null;
  valueX?: string;
  valueY?: string;
  tableReferenceId?: string;
  linkedSpecSheetTypeId?: number;
  linkedSpecSheetPhaseId?: number;
  linkedReferenceId?: string;
};
