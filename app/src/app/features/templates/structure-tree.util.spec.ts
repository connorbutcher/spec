import { CellType } from './models/cell-type.model';
import { StructureNode } from './models/structure-node';
import { TableTemplate } from './models/table-template.model';
import { TemplateCell } from './models/template-cell.model';
import { TemplateSection } from './models/template-section.model';
import { StructureTreeUtil } from './structure-tree.util';
import { TemplateIndexUtil } from './template-index.util';

class TreeSpecHelpers {
  public static cell(id: number, column: number, caption: string | null): TemplateCell {
    return {
      id,
      cellTypeId: 1,
      column,
      columnSpan: 1,
      rowSpan: 1,
      caption,
      isRequired: false,
      configurationOverride: null,
      styleOverride: null,
      columnBlockId: null,
    };
  }

  public static section(
    id: number,
    name: string,
    parentSectionId: number | null,
    sections: TemplateSection[],
    rows: TemplateCell[][],
  ): TemplateSection {
    return {
      id,
      parentSectionId,
      name,
      displayOrder: id,
      role: name === 'Header' ? 'Header' : 'Addable',
      minInstances: 0,
      maxInstances: null,
      initialInstances: 0,
      sections,
      rows: rows.map((cells, index) => ({ id: id * 10 + index, displayOrder: index + 1, cells })),
    };
  }

  public static template(sections: TemplateSection[]): TableTemplate {
    return {
      id: 1,
      sheetTypeId: 1,
      name: 'Spec',
      displayOrder: 1,
      versionId: 1,
      versionNumber: 1,
      orientation: 'Vertical',
      stickyColumnCount: 0,
      isEditable: true,
      versions: [],
      sections,
      columnBlocks: [],
    };
  }

  public static cellType(): CellType | undefined {
    return { id: 1, name: 'Text', kind: 'Text' } as CellType;
  }

  public static labels(nodes: StructureNode[]): string[] {
    return nodes.flatMap((node) => [
      `${node.label}${node.children?.length && !node.expanded ? ' (folded)' : ''}`,
      ...TreeSpecHelpers.labels(node.children ?? []).map((label) => `  ${label}`),
    ]);
  }
}

describe('StructureTreeUtil', () => {
  const joint = TreeSpecHelpers.section(
    3,
    'Bolted joint',
    2,
    [],
    [[TreeSpecHelpers.cell(301, 2, 'Torque'), TreeSpecHelpers.cell(300, 1, 'Fastener')]],
  );
  const system = TreeSpecHelpers.section(2, 'System', null, [joint], []);
  const header = TreeSpecHelpers.section(
    1,
    'Header',
    null,
    [],
    [[TreeSpecHelpers.cell(100, 1, null)]],
  );
  const template = TreeSpecHelpers.template([system, header]);
  const index = TemplateIndexUtil.buildTemplateIndex(template);

  it('shows every level, header first, with rows folded and cells in column order', () => {
    const nodes = StructureTreeUtil.build(template, index, TreeSpecHelpers.cellType, {
      ...StructureTreeUtil.defaultCollapsedKeys(index),
    });

    expect(TreeSpecHelpers.labels(nodes)).toEqual([
      'Spec',
      '  Header',
      '    Row 1 (folded)',
      '      Text',
      '  System',
      '    Bolted joint',
      '      Row 1 (folded)',
      '        Fastener',
      '        Torque',
    ]);
  });

  it('summarises a row by its first cells', () => {
    const row = index.rows.get(30)?.row;
    expect(row && StructureTreeUtil.rowSummary(row, index, TreeSpecHelpers.cellType)).toBe(
      'Fastener, Torque',
    );
  });

  it('lists the nodes above a cell so they can be opened', () => {
    expect(StructureTreeUtil.ancestorKeys({ kind: 'cell', id: 301 }, index)).toEqual([
      'table',
      'section-2',
      'section-3',
      'row-30',
    ]);
  });

  it('only lets sub-sections, rows and column blocks be dragged', () => {
    const nodes = StructureTreeUtil.build(template, index, TreeSpecHelpers.cellType);
    const [table] = nodes;
    const [headerNode, systemNode] = table.children ?? [];
    expect([table.draggable, headerNode.draggable, systemNode.draggable]).toEqual([
      false,
      false,
      false,
    ]);
    expect(systemNode.children?.[0].draggable).toBe(true);
  });
});
