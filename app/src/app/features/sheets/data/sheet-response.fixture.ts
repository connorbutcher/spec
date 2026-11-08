import { TemplateCell } from '../../templates/models/template-cell.model';
import { SheetCell } from '../models/sheet-cell.model';
import { SheetResponse } from '../models/sheet-response.model';
import { SheetRow } from '../models/sheet-row.model';
import { SheetRowsChangeResponse } from '../models/sheet-rows-change-response.model';
import { SheetSection } from '../models/sheet-section.model';
import { Sheet } from '../models/sheet.model';

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

  /** Rows of a sheet as the API sends them after a save or for a quiet read: the rows and what follows from them. */
  public static rowsResponse(sheet: Sheet, rowIds: readonly number[]): SheetRowsChangeResponse {
    const response = SheetResponseFixture.sheetResponse(sheet);
    const rows: SheetRow[] = [];
    const tableIds = new Set<number>();
    response.tables.forEach((table) => {
      const found = SheetResponseFixture.rowsIn(table.sections, rowIds);
      if (found.length > 0) {
        tableIds.add(table.id);
      }
      rows.push(...found);
    });
    return {
      sheetId: sheet.id,
      rows,
      latestVersionNumber: sheet.latestVersionNumber,
      myDraftCount: sheet.myDraftCount,
      otherDrafts: sheet.otherDrafts,
      linkedSources: sheet.linkedSources,
      linkableRows: sheet.tables
        .filter((table) => tableIds.has(table.id))
        .map((table) => ({ sheetTableId: table.id, rows: table.linkableRows })),
      templateCells: response.templateCells,
    };
  }

  private static rowsIn(sections: SheetSection[], rowIds: readonly number[]): SheetRow[] {
    return sections.flatMap((section) => [
      ...section.rows.filter((row) => rowIds.includes(row.id)),
      ...SheetResponseFixture.rowsIn(section.sections, rowIds),
    ]);
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
