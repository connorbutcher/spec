import { Component, computed, inject } from '@angular/core';
import { TreeNode } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TreeModule, TreeNodeSelectEvent } from 'primeng/tree';
import { TemplateSection } from '../models/template-section.model';
import { PanelNavigator } from '../panel-navigator';
import { sectionContents, sectionIcon } from '../section-links.util';
import { describeInstances, instanceTag, isHeader } from '../section-role.util';
import { topLevelOrder } from '../template-layout.util';
import { TemplatesStore } from '../templates.store';

/** A node of the tree: the table, a section, or an "add" entry for the section named in `data`. */
type StructureNode = TreeNode<TemplateSection | null>;

/** The key of the tree's root node, which stands for the table itself. */
const TABLE_KEY = 'table';

/**
 * The table's structure as a tree: the table, its header, its addable sections and the sub-sections
 * inside them. Tags show how each is added on a sheet. Every addable section ends with a
 * "+ Add sub-section" entry, and the table ends with "+ Add section". Selecting a node opens its settings.
 */
@Component({
  selector: 'app-structure-tree',
  imports: [ButtonModule, TagModule, TreeModule],
  templateUrl: './structure-tree.html',
  styleUrl: './structure-tree.scss',
})
export class StructureTree {
  public readonly nodes = computed<StructureNode[]>(() => {
    const template = this.store.template();
    if (!template) {
      return [];
    }
    return [
      {
        key: TABLE_KEY,
        label: template.name,
        data: null,
        expanded: true,
        children: [
          ...this.sectionNodes(topLevelOrder(template.sections)),
          this.addNode(null, 'Add section'),
        ],
      },
    ];
  });

  /** The node for what the panel is showing, as the same object the tree renders. */
  public readonly selected = computed<StructureNode | null>(() => {
    const current = this.navigator.current();
    const key =
      current.kind === 'section'
        ? String(current.id)
        : current.kind === 'template'
          ? TABLE_KEY
          : null;
    return key === null ? null : (this.find(this.nodes(), key) ?? null);
  });

  private readonly store = inject(TemplatesStore);
  private readonly navigator = inject(PanelNavigator);

  public open(event: TreeNodeSelectEvent): void {
    const node = event.node as StructureNode;
    if (node.data) {
      this.navigator.open({ kind: 'section', id: node.data.id });
    } else {
      this.navigator.open({ kind: 'template' });
    }
  }

  /** Adds an addable section (under the table) or a sub-section (under the section the node is for). */
  public async add(node: StructureNode): Promise<void> {
    const id = await this.store.addSection(node.data?.id ?? null);
    if (id !== null) {
      this.navigator.open({ kind: 'section', id });
    }
  }

  public isAdd(node: StructureNode): boolean {
    return node.type === 'add';
  }

  public isBusy(): boolean {
    return this.store.isSaving() || !this.store.canEdit();
  }

  public icon(node: StructureNode): string {
    return node.data ? sectionIcon(node.data) : 'pi-table';
  }

  public tag(node: StructureNode): string {
    return node.data ? instanceTag(node.data) : '';
  }

  public isAddable(node: StructureNode): boolean {
    return node.data ? !isHeader(node.data) : false;
  }

  public hint(node: StructureNode): string {
    return node.data ? `${describeInstances(node.data)} · ${sectionContents(node.data)}` : '';
  }

  private sectionNodes(sections: TemplateSection[]): StructureNode[] {
    return sections.map((section) => ({
      key: String(section.id),
      label: section.name,
      data: section,
      expanded: true,
      children: [
        ...this.sectionNodes(section.sections),
        // The header holds rows only; every addable section can have sub-sections.
        ...(isHeader(section) ? [] : [this.addNode(section, 'Add sub-section')]),
      ],
    }));
  }

  /** An entry that adds under `parent` (null for the table itself). Not selectable: it only adds. */
  private addNode(parent: TemplateSection | null, label: string): StructureNode {
    return {
      key: `add:${parent?.id ?? TABLE_KEY}`,
      label,
      type: 'add',
      data: parent,
      selectable: false,
      leaf: true,
    };
  }

  private find(nodes: StructureNode[], key: string): StructureNode | undefined {
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
