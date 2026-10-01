import { CellType } from './models/cell-type.model';
import { PanelCrumb } from './models/panel-crumb.model';
import { PanelRef } from './models/panel-ref';
import { TableTemplate } from './models/table-template.model';
import { TemplateCell } from './models/template-cell.model';
import { TemplateIndex } from './models/template-index.model';
import { TemplateSection } from './models/template-section.model';

/**
 * The path to a panel through the table: table › sections › row › cell, table › column block, or
 * cell types › type.
 * Returns an empty list when the item no longer exists.
 */
export function panelCrumbs(
  ref: PanelRef,
  template: TableTemplate | null,
  index: TemplateIndex,
  cellType: (id: number) => CellType | undefined,
): PanelCrumb[] {
  const tableCrumb: PanelCrumb[] = template
    ? [{ label: template.name, ref: { kind: 'template' } }]
    : [];
  const sectionCrumbs = (sections: TemplateSection[]): PanelCrumb[] =>
    sections.map((section) => ({ label: section.name, ref: { kind: 'section', id: section.id } }));

  switch (ref.kind) {
    case 'template':
      return tableCrumb;
    case 'section': {
      const entry = index.sections.get(ref.id);
      return entry ? [...tableCrumb, ...sectionCrumbs([...entry.ancestors, entry.section])] : [];
    }
    case 'columnBlock': {
      const entry = index.columnBlocks.get(ref.id);
      return entry ? [...tableCrumb, { label: entry.block.name, ref }] : [];
    }
    case 'row': {
      const entry = index.rows.get(ref.id);
      if (!entry) {
        return [];
      }
      const path = index.sections.get(entry.section.id);
      return [
        ...tableCrumb,
        ...sectionCrumbs([...(path?.ancestors ?? []), entry.section]),
        { label: `Row ${entry.number}`, ref },
      ];
    }
    case 'cell': {
      const entry = index.cells.get(ref.id);
      if (!entry) {
        return [];
      }
      const rowRef: PanelRef = { kind: 'row', id: entry.row.row.id };
      return [
        ...panelCrumbs(rowRef, template, index, cellType),
        { label: cellLabel(entry.cell, index), ref },
      ];
    }
    case 'cellTypes':
      return [{ label: 'Cell types', ref }];
    case 'cellType': {
      const found = cellType(ref.id);
      return found
        ? [
            { label: 'Cell types', ref: { kind: 'cellTypes' } },
            { label: found.name, ref },
          ]
        : [];
    }
  }
}

/** "Column 2", or "Part column 2" for a cell in a column block. */
function cellLabel(cell: TemplateCell, index: TemplateIndex): string {
  const block =
    cell.columnBlockId === null ? undefined : index.columnBlocks.get(cell.columnBlockId);
  return block ? `${block.block.name} column ${cell.column}` : `Column ${cell.column}`;
}
