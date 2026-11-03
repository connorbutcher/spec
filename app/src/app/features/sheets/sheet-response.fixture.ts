import { TemplateCell } from '../templates/models/template-cell.model';
import { SheetCell } from './models/sheet-cell.model';
import { SheetResponse } from './models/sheet-response.model';
import { SheetSection } from './models/sheet-section.model';
import { Sheet } from './models/sheet.model';

/** A sheet as the API sends it: each cell names its template cell, and the sheet lists them once. */
export function sheetResponse(sheet: Sheet): SheetResponse {
  const templateCells = new Map<number, TemplateCell>();

  const withoutTemplate = ({ template, ...cell }: SheetCell): SheetCell => {
    templateCells.set(template.id, template);
    return { ...cell, templateCellId: template.id } as unknown as SheetCell;
  };

  const section = (from: SheetSection): SheetSection => ({
    ...from,
    rows: from.rows.map((row) => ({ ...row, cells: row.cells.map(withoutTemplate) })),
    sections: from.sections.map(section),
  });

  const tables = sheet.tables.map((table) => ({ ...table, sections: table.sections.map(section) }));
  return { ...sheet, tables, templateCells: [...templateCells.values()] };
}
