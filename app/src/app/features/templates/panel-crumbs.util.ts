import { CellType } from './models/cell-type.model';
import { PanelCrumb } from './models/panel-crumb.model';
import { PanelRef } from './models/panel-ref';
import { TableTemplate } from './models/table-template.model';
import { TemplateIndex } from './models/template-index.model';
import { TemplateSection } from './models/template-section.model';
import { StructureIcons } from './structure-icons';
import { StructureTreeUtil } from './structure-tree.util';

export class PanelCrumbsUtil {
  /**
   * The path to a panel through the table: table › sections › row › cell, table › column block, or
   * cell types › type.
   * Returns an empty list when the item no longer exists.
   */
  public static panelCrumbs(
    ref: PanelRef,
    template: TableTemplate | null,
    index: TemplateIndex,
    cellType: (id: number) => CellType | undefined,
  ): PanelCrumb[] {
    const tableCrumb: PanelCrumb[] = template
      ? [{ label: template.name, ref: { kind: 'template' }, level: 'table' }]
      : [];

    switch (ref.kind) {
      case 'template':
        return tableCrumb;
      case 'section': {
        const entry = index.sections.get(ref.id);
        return entry
          ? [...tableCrumb, ...PanelCrumbsUtil.sectionCrumbs([...entry.ancestors, entry.section])]
          : [];
      }
      case 'columnBlock': {
        const entry = index.columnBlocks.get(ref.id);
        return entry ? [...tableCrumb, { label: entry.block.name, ref, level: 'columnBlock' }] : [];
      }
      case 'row': {
        const entry = index.rows.get(ref.id);
        if (!entry) {
          return [];
        }
        const path = index.sections.get(entry.section.id);
        return [
          ...tableCrumb,
          ...PanelCrumbsUtil.sectionCrumbs([...(path?.ancestors ?? []), entry.section]),
          { label: `Row ${entry.number}`, ref, level: 'row' },
        ];
      }
      case 'cell': {
        const entry = index.cells.get(ref.id);
        if (!entry) {
          return [];
        }
        const rowRef: PanelRef = { kind: 'row', id: entry.row.row.id };
        return [
          ...PanelCrumbsUtil.panelCrumbs(rowRef, template, index, cellType),
          { label: StructureTreeUtil.cellName(entry.cell, cellType), ref, level: 'cell' },
        ];
      }
      case 'cellTypes':
        return [{ label: 'Cell types', ref, level: 'cellTypes' }];
      case 'cellType': {
        const found = cellType(ref.id);
        return found
          ? [
              { label: 'Cell types', ref: { kind: 'cellTypes' }, level: 'cellTypes' },
              { label: found.name, ref, level: 'cellType' },
            ]
          : [];
      }
    }
  }

  private static sectionCrumbs(sections: TemplateSection[]): PanelCrumb[] {
    return sections.map((section) => ({
      label: section.name,
      ref: { kind: 'section', id: section.id },
      level: StructureIcons.sectionLevel(section),
    }));
  }
}
