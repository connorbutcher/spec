import { TemplateCell } from '../templates/models/template-cell.model';
import { SheetCell } from './models/sheet-cell.model';
import { SheetResponse } from './models/sheet-response.model';
import { SheetSection } from './models/sheet-section.model';
import { Sheet } from './models/sheet.model';

export class SheetResponseFixture {
  /** A sheet as the API sends it: each cell names its template cell, and the sheet lists them once. */
  public static sheetResponse(sheet: Sheet): SheetResponse {
    const templateCells = new Map<number, TemplateCell>();
    const tables = sheet.tables.map((table) => ({
      ...table,
      sections: table.sections.map((section) => SheetResponseFixture.withoutTemplates(section, templateCells)),
    }));
    return { ...sheet, tables, templateCells: [...templateCells.values()] };
  }

  private static withoutTemplates(from: SheetSection, templateCells: Map<number, TemplateCell>): SheetSection {
    return {
      ...from,
      rows: from.rows.map((row) => ({
        ...row,
        cells: row.cells.map((cell) => SheetResponseFixture.withoutTemplate(cell, templateCells)),
      })),
      sections: from.sections.map((section) => SheetResponseFixture.withoutTemplates(section, templateCells)),
    };
  }

  private static withoutTemplate({ template, ...cell }: SheetCell, templateCells: Map<number, TemplateCell>): SheetCell {
    templateCells.set(template.id, template);
    return { ...cell, templateCellId: template.id } as unknown as SheetCell;
  }
}
