import { SectionTone } from './models/section-tone';
import { SheetCell } from './models/sheet-cell.model';
import { SheetIndex } from './models/sheet-index.model';
import { SheetRow } from './models/sheet-row.model';
import { SheetSection } from './models/sheet-section.model';
import { SheetTable } from './models/sheet-table.model';
import { Sheet } from './models/sheet.model';

/** Indexes every table, section, row and cell of the sheet by id. */
export function buildSheetIndex(sheet: Sheet | null): SheetIndex {
  const index = {
    tables: new Map<number, SheetTable>(),
    sections: new Map<number, SheetSection>(),
    rows: new Map<number, SheetRow>(),
    cells: new Map<number, SheetCell>(),
    sectionParent: new Map<number, number | null>(),
    sectionTable: new Map<number, number>(),
    rowSection: new Map<number, number>(),
  } satisfies SheetIndex;

  const addSection = (section: SheetSection, tableId: number, parentId: number | null): void => {
    index.sections.set(section.id, section);
    index.sectionParent.set(section.id, parentId);
    index.sectionTable.set(section.id, tableId);
    for (const row of section.rows) {
      index.rows.set(row.id, row);
      index.rowSection.set(row.id, section.id);
      for (const cell of row.cells) {
        index.cells.set(cell.id, cell);
      }
    }
    for (const child of section.sections) {
      addSection(child, tableId, section.id);
    }
  };

  for (const table of sheet?.tables ?? []) {
    index.tables.set(table.id, table);
    for (const section of table.sections) {
      addSection(section, table.id, null);
    }
  }
  return index;
}

/** The sheet's cells with the given ids. */
export function cellsById(sheet: Sheet, cellIds: number[]): Set<object> {
  if (cellIds.length === 0) {
    return new Set();
  }
  const cells = buildSheetIndex(sheet).cells;
  return new Set(cellIds.map((id) => cells.get(id)).filter((cell) => cell !== undefined));
}

/** The first id in `after` that `before` doesn't have: the item a change just added. */
export function addedId(
  before: ReadonlyMap<number, unknown>,
  after: ReadonlyMap<number, unknown>,
): number | undefined {
  return [...after.keys()].find((id) => !before.has(id));
}

/** The section's ancestors from the top level down to (not including) the section itself. */
export function sectionAncestors(index: SheetIndex, sectionId: number): SheetSection[] {
  const ancestors: SheetSection[] = [];
  let parentId = index.sectionParent.get(sectionId) ?? null;
  while (parentId !== null) {
    const parent = index.sections.get(parentId);
    if (!parent) {
      break;
    }
    ancestors.unshift(parent);
    parentId = index.sectionParent.get(parentId) ?? null;
  }
  return ancestors;
}

/** The sections at the same level as `sectionId`, in display order, including itself. */
export function sectionSiblings(index: SheetIndex, sectionId: number): SheetSection[] {
  const parentId = index.sectionParent.get(sectionId) ?? null;
  if (parentId !== null) {
    return index.sections.get(parentId)?.sections ?? [];
  }
  const table = index.tables.get(index.sectionTable.get(sectionId) ?? -1);
  return table?.sections ?? [];
}

/** Whether a section is a group: it holds other sections, or can. The header never is. */
export function isGroupSection(section: SheetSection): boolean {
  return (
    section.role !== 'Header' && (section.sections.length > 0 || section.addableSections.length > 0)
  );
}

/** The tone of a row's cells, from the section the row is in. Deeper groups are lighter. */
export function sectionTone(index: SheetIndex, rowId: number): SectionTone {
  const section = index.sections.get(index.rowSection.get(rowId) ?? -1);
  if (section === undefined) {
    return 'plain';
  }
  if (section.role === 'Header') {
    return 'header';
  }
  if (!isGroupSection(section)) {
    return 'plain';
  }
  const depth = Math.min(sectionAncestors(index, section.id).length, 2) as 0 | 1 | 2;
  return `group-${depth}`;
}
