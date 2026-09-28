/** A cell in a row. `column` is 1-based; the spans map onto CSS grid spans. */
export interface TemplateCell {
  id: number;
  cellTypeId: number;
  column: number;
  rowSpan: number;
  columnSpan: number;
  caption: string | null;
  isRequired: boolean;
}
