import { TemplateCell } from '../../templates/models/template-cell.model';
import { SheetCellTemplateRef } from '../models/sheet-cell-template-ref.model';
import { SheetCell } from '../models/sheet-cell.model';
import { SheetResponse } from '../models/sheet-response.model';
import { SheetRow } from '../models/sheet-row.model';
import { SheetRowsChangeResponse } from '../models/sheet-rows-change-response.model';
import { SheetRowsChange } from '../models/sheet-rows-change.model';
import { SheetSection } from '../models/sheet-section.model';
import { Sheet } from '../models/sheet.model';

export class SheetResponseUtil {
  /**
   * The sheet with each cell given its template cell from the one list the response sends them in. Cells
   * built from the same template cell share its object.
   */
  public static withTemplateCells(response: SheetResponse): Sheet {
    const { templateCells, ...sheet } = response;
    const byId = SheetResponseUtil.byId(templateCells);
    return {
      ...sheet,
      tables: sheet.tables.map((table) => ({
        ...table,
        sections: table.sections.map((section) => SheetResponseUtil.withTemplates(section, byId)),
      })),
    };
  }

  /** Some rows, each cell given its template cell, as `withTemplateCells` does for a whole sheet. */
  public static rowsWithTemplateCells(response: SheetRowsChangeResponse): SheetRowsChange {
    const { templateCells, ...change } = response;
    const byId = SheetResponseUtil.byId(templateCells);
    return { ...change, rows: change.rows.map((row) => SheetResponseUtil.withRowTemplates(row, byId)) };
  }

  private static byId(templateCells: TemplateCell[]): ReadonlyMap<number, TemplateCell> {
    return new Map(templateCells.map((templateCell) => [templateCell.id, templateCell]));
  }

  private static withTemplates(from: SheetSection, byId: ReadonlyMap<number, TemplateCell>): SheetSection {
    return {
      ...from,
      rows: from.rows.map((row) => SheetResponseUtil.withRowTemplates(row, byId)),
      sections: from.sections.map((section) => SheetResponseUtil.withTemplates(section, byId)),
    };
  }

  private static withRowTemplates(row: SheetRow, byId: ReadonlyMap<number, TemplateCell>): SheetRow {
    return { ...row, cells: row.cells.map((cell) => SheetResponseUtil.withTemplate(cell, byId)) };
  }

  private static withTemplate(cell: SheetCell, byId: ReadonlyMap<number, TemplateCell>): SheetCell {
    const { templateCellId, ...rest } = cell as SheetCell & SheetCellTemplateRef;
    const template = byId.get(templateCellId);
    if (template === undefined) {
      throw new Error(`The sheet has no template cell ${templateCellId}.`);
    }
    return { ...rest, template };
  }
}
