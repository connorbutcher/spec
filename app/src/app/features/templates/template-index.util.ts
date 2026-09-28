import { CellEntry } from './models/cell-entry.model';
import { RowEntry } from './models/row-entry.model';
import { SectionEntry } from './models/section-entry.model';
import { TableTemplate } from './models/table-template.model';
import { TemplateIndex } from './models/template-index.model';
import { TemplateSection } from './models/template-section.model';

/** Indexes every section, row and cell of a template by id. */
export function buildTemplateIndex(template: TableTemplate | null): TemplateIndex {
  const sections = new Map<number, SectionEntry>();
  const rows = new Map<number, RowEntry>();
  const cells = new Map<number, CellEntry>();

  const visit = (siblings: TemplateSection[], ancestors: TemplateSection[]): void => {
    for (const section of siblings) {
      sections.set(section.id, { section, depth: ancestors.length + 1, ancestors, siblings });
      section.rows.forEach((row, index) => {
        const rowEntry: RowEntry = { row, section, number: index + 1 };
        rows.set(row.id, rowEntry);
        for (const cell of row.cells) {
          cells.set(cell.id, { cell, row: rowEntry });
        }
      });
      visit(section.sections, [...ancestors, section]);
    }
  };

  visit(template?.sections ?? [], []);
  return { sections, rows, cells };
}

/** The ids in `after` that aren't in `before`, used to find what a create call added. */
export function addedIds(
  before: ReadonlyMap<number, unknown>,
  after: ReadonlyMap<number, unknown>,
): number[] {
  return [...after.keys()].filter((id) => !before.has(id));
}
