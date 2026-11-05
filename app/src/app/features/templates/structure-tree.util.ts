import { CellKinds } from './models/cell-kinds';
import { CellType } from './models/cell-type.model';
import { PanelRef } from './models/panel-ref';
import { StructureItem } from './models/structure-item.model';
import { StructureNode } from './models/structure-node';
import { TableTemplate } from './models/table-template.model';
import { TemplateCell } from './models/template-cell.model';
import { TemplateIndex } from './models/template-index.model';
import { TemplateRow } from './models/template-row.model';
import { TemplateSection } from './models/template-section.model';
import { StructureIcons } from './structure-icons';
import { TemplateLayoutUtil } from './template-layout.util';

/** The key of the node that groups a horizontal table's column blocks. */
export const COLUMN_BLOCKS_KEY = 'column-blocks';

/** How many cell names a row's summary lists before it just counts the rest. */
const ROW_SUMMARY_NAMES = 2;

/** How long a cell name in a row summary may be before it is cut short. */
const SUMMARY_NAME_LENGTH = 18;

/**
 * Builds the designer's structure tree: the table, its header and sections, their sub-sections, rows
 * and cells, and a horizontal table's column blocks. Every node's key is derived from what it opens,
 * so the tree can find the node for whatever is selected.
 */
export class StructureTreeUtil {
  public static build(
    template: TableTemplate,
    index: TemplateIndex,
    cellType: (id: number) => CellType | undefined,
    collapsedKeys: Readonly<Record<string, boolean>> = {},
  ): StructureNode[] {
    const sections = TemplateLayoutUtil.topLevelOrder(template.sections).map((section) =>
      StructureTreeUtil.sectionNode(section, index, cellType),
    );
    const blocks: StructureNode[] =
      template.orientation === 'Horizontal' ? [StructureTreeUtil.columnBlocksNode(template)] : [];

    const nodes: StructureNode[] = [
      {
        key: 'table',
        label: template.name,
        data: {
          ref: { kind: 'template' },
          level: 'table',
          icon: StructureIcons.icon('table'),
          meta: '',
        },
        draggable: false,
        droppable: false,
        children: [...sections, ...blocks],
      },
    ];
    StructureTreeUtil.applyExpansion(nodes, collapsedKeys);
    return nodes;
  }

  /** The tree key for a panel, or null when the panel has no node (the cell types). */
  public static keyFor(ref: PanelRef): string | null {
    switch (ref.kind) {
      case 'template':
        return 'table';
      case 'section':
        return `section-${ref.id}`;
      case 'row':
        return `row-${ref.id}`;
      case 'cell':
        return `cell-${ref.id}`;
      case 'columnBlock':
        return `block-${ref.id}`;
      default:
        return null;
    }
  }

  /** The keys of the nodes above a panel's node, from the table down, so they can be expanded. */
  public static ancestorKeys(ref: PanelRef, index: TemplateIndex): string[] {
    switch (ref.kind) {
      case 'section': {
        const entry = index.sections.get(ref.id);
        return ['table', ...(entry?.ancestors ?? []).map((section) => `section-${section.id}`)];
      }
      case 'row': {
        const entry = index.rows.get(ref.id);
        return entry
          ? ['table', ...StructureTreeUtil.sectionKeys(entry.section.id, index)]
          : ['table'];
      }
      case 'cell': {
        const entry = index.cells.get(ref.id);
        return entry
          ? [
              'table',
              ...StructureTreeUtil.sectionKeys(entry.row.section.id, index),
              `row-${entry.row.row.id}`,
            ]
          : ['table'];
      }
      case 'columnBlock':
        return ['table', COLUMN_BLOCKS_KEY];
      default:
        return [];
    }
  }

  /** Rows start folded, so the tree opens at sections; everything else starts open. */
  public static defaultCollapsedKeys(index: TemplateIndex): Record<string, boolean> {
    const keys: Record<string, boolean> = {};
    for (const id of index.rows.keys()) {
      keys[`row-${id}`] = true;
    }
    return keys;
  }

  /** A row's cells in the order they read: the row's own cells, then each column block's, by column. */
  public static orderedCells(row: TemplateRow, index: TemplateIndex): TemplateCell[] {
    return [...row.cells].sort(
      (a, b) =>
        StructureTreeUtil.blockNumber(a, index) - StructureTreeUtil.blockNumber(b, index) ||
        a.column - b.column,
    );
  }

  /** "Fastener, Thread size +2", from the cells' captions or, without one, their type names. */
  public static rowSummary(
    row: TemplateRow,
    index: TemplateIndex,
    cellType: (id: number) => CellType | undefined,
  ): string {
    const cells = StructureTreeUtil.orderedCells(row, index);
    if (cells.length === 0) {
      return 'empty';
    }
    const names = cells
      .slice(0, ROW_SUMMARY_NAMES)
      .map((cell) => StructureTreeUtil.shorten(StructureTreeUtil.cellName(cell, cellType)));
    const rest = cells.length - names.length;
    return rest > 0 ? `${names.join(', ')} +${rest}` : names.join(', ');
  }

  /** A cell's caption, or its type's name when it has none. */
  public static cellName(
    cell: TemplateCell,
    cellType: (id: number) => CellType | undefined,
  ): string {
    return cell.caption || cellType(cell.cellTypeId)?.name || 'Cell';
  }

  /** The keys of a section and the sections above it, from the top level down. */
  private static sectionKeys(sectionId: number, index: TemplateIndex): string[] {
    const entry = index.sections.get(sectionId);
    return entry
      ? [...entry.ancestors, entry.section].map((section) => `section-${section.id}`)
      : [];
  }

  /** 0 for a row's own cell, or its column block's place from the left. */
  private static blockNumber(cell: TemplateCell, index: TemplateIndex): number {
    return cell.columnBlockId === null
      ? 0
      : (index.columnBlocks.get(cell.columnBlockId)?.number ?? 0);
  }

  private static shorten(name: string): string {
    return name.length > SUMMARY_NAME_LENGTH ? `${name.slice(0, SUMMARY_NAME_LENGTH - 1)}…` : name;
  }

  /** Opens every node with children unless its key is folded. */
  private static applyExpansion(
    nodes: StructureNode[],
    collapsedKeys: Readonly<Record<string, boolean>>,
  ): void {
    for (const node of nodes) {
      if (node.children && node.children.length > 0) {
        node.expanded = !(node.key && collapsedKeys[node.key]);
        StructureTreeUtil.applyExpansion(node.children, collapsedKeys);
      }
    }
  }

  private static sectionNode(
    section: TemplateSection,
    index: TemplateIndex,
    cellType: (id: number) => CellType | undefined,
  ): StructureNode {
    const level = StructureIcons.sectionLevel(section);
    const rows = section.rows.map((row, position) =>
      StructureTreeUtil.rowNode(row, position + 1, index, cellType),
    );
    const children = section.sections.map((child) =>
      StructureTreeUtil.sectionNode(child, index, cellType),
    );

    return {
      key: `section-${section.id}`,
      label: section.name,
      data: {
        ref: { kind: 'section', id: section.id },
        level,
        icon: StructureIcons.icon(level),
        meta: StructureTreeUtil.sectionMeta(section),
      },
      // Only sub-sections have an order to change; the header and top-level sections stay put.
      draggable: level === 'subSection',
      droppable: false,
      children: [...rows, ...children],
    };
  }

  private static rowNode(
    row: TemplateRow,
    number: number,
    index: TemplateIndex,
    cellType: (id: number) => CellType | undefined,
  ): StructureNode {
    return {
      key: `row-${row.id}`,
      label: `Row ${number}`,
      data: {
        ref: { kind: 'row', id: row.id },
        level: 'row',
        icon: StructureIcons.icon('row'),
        meta: StructureTreeUtil.rowSummary(row, index, cellType),
      },
      draggable: true,
      droppable: false,
      children: StructureTreeUtil.orderedCells(row, index).map((cell) =>
        StructureTreeUtil.cellNode(cell, index, cellType),
      ),
    };
  }

  private static cellNode(
    cell: TemplateCell,
    index: TemplateIndex,
    cellType: (id: number) => CellType | undefined,
  ): StructureNode {
    const type = cellType(cell.cellTypeId);
    const block =
      cell.columnBlockId === null ? undefined : index.columnBlocks.get(cell.columnBlockId);
    const meta = [block?.block.name ?? '', cell.caption ? (type?.name ?? '') : '']
      .filter(Boolean)
      .join(' · ');

    return {
      key: `cell-${cell.id}`,
      label: StructureTreeUtil.cellName(cell, cellType),
      data: {
        ref: { kind: 'cell', id: cell.id },
        level: 'cell',
        icon: CellKinds.cellKindInfo(type?.kind ?? 'Text').icon,
        meta,
      },
      leaf: true,
      draggable: false,
      droppable: false,
    };
  }

  private static columnBlocksNode(template: TableTemplate): StructureNode {
    return {
      key: COLUMN_BLOCKS_KEY,
      label: 'Column blocks',
      data: {
        ref: null,
        level: 'columnBlocks',
        icon: StructureIcons.icon('columnBlocks'),
        meta: `${template.columnBlocks.length}`,
      },
      selectable: false,
      draggable: false,
      droppable: false,
      children: [...template.columnBlocks]
        .sort((a, b) => a.displayOrder - b.displayOrder)
        .map((block) => ({
          key: `block-${block.id}`,
          label: block.name,
          data: {
            ref: { kind: 'columnBlock', id: block.id },
            level: 'columnBlock',
            icon: StructureIcons.icon('columnBlock'),
            meta: '',
          },
          leaf: true,
          draggable: true,
          droppable: false,
        })),
    };
  }

  /** "2 rows · 4 subs", or "empty". */
  private static sectionMeta(section: TemplateSection): string {
    const parts = [
      section.rows.length > 0 ? StructureTreeUtil.plural(section.rows.length, 'row') : '',
      section.sections.length > 0 ? StructureTreeUtil.plural(section.sections.length, 'sub') : '',
    ].filter(Boolean);
    return parts.length > 0 ? parts.join(' · ') : 'empty';
  }

  private static plural(count: number, noun: string): string {
    return `${count} ${noun}${count === 1 ? '' : 's'}`;
  }
}
