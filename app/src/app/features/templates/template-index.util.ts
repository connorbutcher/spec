import { WritableMaps } from '../../shared/models/writable-maps';
import { CellEntry } from './models/cell-entry.model';
import { ColumnBlockEntry } from './models/column-block-entry.model';
import { RowEntry } from './models/row-entry.model';
import { SectionEntry } from './models/section-entry.model';
import { TableTemplate } from './models/table-template.model';
import { TemplateIndex } from './models/template-index.model';
import { TemplateSection } from './models/template-section.model';

export class TemplateIndexUtil {
  /** Indexes every section, column block, row and cell of a template by id. */
  public static buildTemplateIndex(template: TableTemplate | null): TemplateIndex {
    const index: WritableMaps<TemplateIndex> = {
      sections: new Map<number, SectionEntry>(),
      columnBlocks: new Map<number, ColumnBlockEntry>(),
      rows: new Map<number, RowEntry>(),
      cells: new Map<number, CellEntry>(),
    };
    TemplateIndexUtil.addSections(index, template?.sections ?? [], []);
    [...(template?.columnBlocks ?? [])]
      .sort((a, b) => a.displayOrder - b.displayOrder)
      .forEach((block, number) => index.columnBlocks.set(block.id, { block, number: number + 1 }));
    return index;
  }

  /** The ids in `after` that aren't in `before`, used to find what a create call added. */
  public static addedIds(before: ReadonlyMap<number, unknown>, after: ReadonlyMap<number, unknown>): number[] {
    return [...after.keys()].filter((id) => !before.has(id));
  }

  private static addSections(index: WritableMaps<TemplateIndex>, siblings: TemplateSection[], ancestors: TemplateSection[]): void {
    for (const section of siblings) {
      index.sections.set(section.id, { section, depth: ancestors.length + 1, ancestors, siblings });
      section.rows.forEach((row, number) => {
        const rowEntry: RowEntry = { row, section, number: number + 1 };
        index.rows.set(row.id, rowEntry);
        for (const cell of row.cells) {
          index.cells.set(cell.id, { cell, row: rowEntry });
        }
      });
      TemplateIndexUtil.addSections(index, section.sections, [...ancestors, section]);
    }
  }
}
