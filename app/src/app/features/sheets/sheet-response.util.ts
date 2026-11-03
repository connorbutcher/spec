import { SheetCellTemplateRef } from './models/sheet-cell-template-ref.model';
import { SheetCell } from './models/sheet-cell.model';
import { SheetResponse } from './models/sheet-response.model';
import { SheetSection } from './models/sheet-section.model';
import { Sheet } from './models/sheet.model';

/**
 * The sheet with each cell given its template cell from the one list the response sends them in. Cells
 * built from the same template cell share its object.
 */
export function withTemplateCells(response: SheetResponse): Sheet {
  const { templateCells, ...sheet } = response;
  const byId = new Map(templateCells.map((templateCell) => [templateCell.id, templateCell]));

  const withTemplate = (cell: SheetCell): SheetCell => {
    const { templateCellId, ...rest } = cell as SheetCell & SheetCellTemplateRef;
    const template = byId.get(templateCellId);
    if (template === undefined) {
      throw new Error(`The sheet has no template cell ${templateCellId}.`);
    }
    return { ...rest, template };
  };

  const section = (from: SheetSection): SheetSection => ({
    ...from,
    rows: from.rows.map((row) => ({ ...row, cells: row.cells.map(withTemplate) })),
    sections: from.sections.map(section),
  });

  return {
    ...sheet,
    tables: sheet.tables.map((table) => ({ ...table, sections: table.sections.map(section) })),
  };
}
