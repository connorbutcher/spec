import { Component, computed, inject } from '@angular/core';
import { TreeNode } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { TreeModule, TreeNodeSelectEvent } from 'primeng/tree';
import { OutlineItem } from '../models/outline-item';
import { PanelRef } from '../models/panel-ref';
import { TemplateSection } from '../models/template-section.model';
import { PanelNavigator } from '../panel-navigator';
import { isHeader } from '../section-role.util';
import { topLevelOrder } from '../template-layout.util';
import { TemplatesStore } from '../templates.store';

type OutlineNode = TreeNode<OutlineItem | null>;

/** The key of the outline's root node, which stands for the table itself. */
const TABLE_KEY = 'table';

/** The key of the node that groups a horizontal table's column blocks. */
const BLOCKS_KEY = 'column-blocks';

/**
 * The table's structure as a plain outline: the table, its header, its addable sections and the
 * sub-sections inside them, and for a horizontal table its column blocks. Selecting a node opens its
 * settings; hovering or selecting an addable node shows a small "+" to add a sub-section to it. The
 * outline ends with "Add section", and "Add column block" for a horizontal table.
 */
@Component({
  selector: 'app-structure-outline',
  imports: [ButtonModule, TreeModule],
  templateUrl: './structure-outline.html',
  styleUrl: './structure-outline.scss',
})
export class StructureOutline {
  public readonly isHorizontal = computed(
    () => this.store.template()?.orientation === 'Horizontal',
  );

  public readonly nodes = computed<OutlineNode[]>(() => {
    const template = this.store.template();
    if (!template) {
      return [];
    }
    const blocks: OutlineNode[] = this.isHorizontal()
      ? [
          {
            key: BLOCKS_KEY,
            label: 'Column blocks',
            data: null,
            selectable: false,
            expanded: true,
            styleClass: 'group',
            children: [...template.columnBlocks]
              .sort((a, b) => a.displayOrder - b.displayOrder)
              .map((block) => ({
                key: `block-${block.id}`,
                label: block.name,
                data: { kind: 'columnBlock', block },
              })),
          },
        ]
      : [];
    return [
      {
        key: TABLE_KEY,
        label: template.name,
        data: null,
        expanded: true,
        children: [...this.sectionNodes(topLevelOrder(template.sections)), ...blocks],
      },
    ];
  });

  /** The node for what the panel is showing, as the same object the tree renders. */
  public readonly selected = computed<OutlineNode | null>(() => {
    const key = keyFor(this.navigator.current());
    return key === null ? null : (this.find(this.nodes(), key) ?? null);
  });

  private readonly store = inject(TemplatesStore);
  private readonly navigator = inject(PanelNavigator);

  public open(event: TreeNodeSelectEvent): void {
    const item = (event.node as OutlineNode).data;
    if (!item) {
      this.navigator.open({ kind: 'template' });
    } else if (item.kind === 'section') {
      this.navigator.open({ kind: 'section', id: item.section.id });
    } else {
      this.navigator.open({ kind: 'columnBlock', id: item.block.id });
    }
  }

  public isBusy(): boolean {
    return this.store.isSaving() || !this.store.canEdit();
  }

  /** Only addable sections can hold sub-sections; the header holds rows only. */
  public canAddTo(node: OutlineNode): boolean {
    return node.data?.kind === 'section' && !isHeader(node.data.section);
  }

  /** Adds a sub-section to the node's section. */
  public addTo(node: OutlineNode, event: Event): void {
    if (node.data?.kind === 'section') {
      void this.add(node.data.section, event);
    }
  }

  /** Adds a sub-section to `parent`, or an addable section to the table with null. */
  public async add(parent: TemplateSection | null, event?: Event): Promise<void> {
    event?.stopPropagation();
    const id = await this.store.addSection(parent?.id ?? null);
    if (id !== null) {
      this.navigator.open({ kind: 'section', id });
    }
  }

  public async addColumnBlock(): Promise<void> {
    const id = await this.store.addColumnBlock();
    if (id !== null) {
      this.navigator.open({ kind: 'columnBlock', id });
    }
  }

  private sectionNodes(sections: TemplateSection[]): OutlineNode[] {
    return sections.map((section) => ({
      key: String(section.id),
      label: section.name,
      data: { kind: 'section', section },
      expanded: true,
      children: this.sectionNodes(section.sections),
    }));
  }

  private find(nodes: OutlineNode[], key: string): OutlineNode | undefined {
    for (const node of nodes) {
      if (node.key === key) {
        return node;
      }
      const found = this.find(node.children ?? [], key);
      if (found) {
        return found;
      }
    }
    return undefined;
  }
}

/** The outline key of the node for a panel, or null when the panel has no node. */
function keyFor(ref: PanelRef): string | null {
  switch (ref.kind) {
    case 'template':
      return TABLE_KEY;
    case 'section':
      return String(ref.id);
    case 'columnBlock':
      return `block-${ref.id}`;
    default:
      return null;
  }
}
