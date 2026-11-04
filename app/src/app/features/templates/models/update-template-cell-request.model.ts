/** The body for changing a cell. Every field is sent each time. */
export type UpdateTemplateCellRequest = {
  cellTypeId: number;
  column: number;
  rowSpan: number;
  columnSpan: number;
  caption: string | null;
  isRequired: boolean;
}
