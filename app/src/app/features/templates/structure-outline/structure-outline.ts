import { Component, computed, inject } from '@angular/core';
import { TreeNode } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { TreeModule, TreeNodeSelectEvent } from 'primeng/tree';
import { TemplateSection } from '../models/template-section.model';
import { PanelNavigator } from '../panel-navigator';
import { isHeader } from '../section-role.util';
import { topLevelOrder } from '../template-layout.util';
import { TemplatesStore } from '../templates.store';

type OutlineNode = TreeNode<TemplateSection | null>;

/** The key of the outline's root node, which stands for the table itself. */
const TABLE_KEY = 'table';

/**
 * The table's structure as a plain outline: the table, its header, its addable sections and the
 * sub-sections inside them. Selecting a node opens its settings; hovering or selecting an addable
 * node shows a small "+" to add a sub-section to it. The outline ends with "Add section".
 */
@Component({
  selector: 'app-structure-outline',
  imports: [ButtonModule, TreeModule],
  templateUrl: './structure-outline.html',
  styleUrl: './structure-outline.scss',
})
export class StructureOutline {
  public readonly nodes = computed<OutlineNode[]>(() => {
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
        children: this.sectionNodes(topLevelOrder(template.sections)),
      },
    ];
  });

  /** The node for what the panel is showing, as the same object the tree renders. */
  public readonly selected = computed<OutlineNode | null>(() => {
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
    const node = event.node as OutlineNode;
    this.navigator.open(node.data ? { kind: 'section', id: node.data.id } : { kind: 'template' });
  }

  public isBusy(): boolean {
    return this.store.isSaving() || !this.store.canEdit();
  }

  /** Only addable sections can hold sub-sections; the header holds rows only. */
  public canAddTo(node: OutlineNode): boolean {
    return node.data ? !isHeader(node.data) : false;
  }

  /** Adds a sub-section to the node's section, or an addable section to the table with null. */
  public async add(parent: TemplateSection | null, event?: Event): Promise<void> {
    event?.stopPropagation();
    const id = await this.store.addSection(parent?.id ?? null);
    if (id !== null) {
      this.navigator.open({ kind: 'section', id });
    }
  }

  private sectionNodes(sections: TemplateSection[]): OutlineNode[] {
    return sections.map((section) => ({
      key: String(section.id),
      label: section.name,
      data: section,
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
