import { Component, computed, inject } from '@angular/core';
import { TreeNode } from 'primeng/api';
import { TagModule } from 'primeng/tag';
import { TreeModule, TreeNodeSelectEvent } from 'primeng/tree';
import { TemplateSection } from '../models/template-section.model';
import { PanelNavigator } from '../panel-navigator';
import { sectionContents, sectionIcon } from '../section-links.util';
import { describeInstances, instanceTag, isHeader } from '../section-role.util';
import { topLevelOrder } from '../template-layout.util';
import { TemplatesStore } from '../templates.store';

/** The key of the tree's root node, which stands for the table itself. */
const TABLE_KEY = 'table';

/**
 * The table's structure as a tree: the table, its header, its addable sections and the sub-sections
 * inside them. Tags show how each can be added on a sheet. Selecting a node opens its settings.
 */
@Component({
  selector: 'app-structure-tree',
  imports: [TagModule, TreeModule],
  templateUrl: './structure-tree.html',
  styleUrl: './structure-tree.scss',
})
export class StructureTree {
  public readonly nodes = computed<TreeNode<TemplateSection | null>[]>(() => {
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
  public readonly selected = computed<TreeNode<TemplateSection | null> | null>(() => {
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
    const node = event.node as TreeNode<TemplateSection | null>;
    if (node.data) {
      this.navigator.open({ kind: 'section', id: node.data.id });
    } else {
      this.navigator.open({ kind: 'template' });
    }
  }

  public icon(node: TreeNode<TemplateSection | null>): string {
    return node.data ? sectionIcon(node.data) : 'pi-table';
  }

  public tag(node: TreeNode<TemplateSection | null>): string {
    return node.data ? instanceTag(node.data) : '';
  }

  public isAddable(node: TreeNode<TemplateSection | null>): boolean {
    return node.data ? !isHeader(node.data) : false;
  }

  public hint(node: TreeNode<TemplateSection | null>): string {
    return node.data ? `${describeInstances(node.data)} · ${sectionContents(node.data)}` : '';
  }

  private sectionNodes(sections: TemplateSection[]): TreeNode<TemplateSection | null>[] {
    return sections.map((section) => ({
      key: String(section.id),
      label: section.name,
      data: section,
      expanded: true,
      children: this.sectionNodes(section.sections),
    }));
  }

  private find(
    nodes: TreeNode<TemplateSection | null>[],
    key: string,
  ): TreeNode<TemplateSection | null> | undefined {
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
