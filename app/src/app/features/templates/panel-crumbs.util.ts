import { CellType } from './models/cell-type.model';
import { PanelCrumb } from './models/panel-crumb.model';
import { PanelRef } from './models/panel-ref';
import { TableTemplate } from './models/table-template.model';
import { TemplateIndex } from './models/template-index.model';
import { TemplateSection } from './models/template-section.model';

/**
 * The path to a panel through the table: table › sections › row › cell, or cell types › type.
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
        { label: `Column ${entry.cell.column}`, ref },
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
