import {
  afterRenderEffect,
  Component,
  computed,
  effect,
  ElementRef,
  inject,
  signal,
  untracked,
} from '@angular/core';
import { TreeDragDropService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import {
  TreeModule,
  TreeNodeCollapseEvent,
  TreeNodeDropEvent,
  TreeNodeExpandEvent,
  TreeNodeSelectEvent,
} from 'primeng/tree';
import { CellTypesStore } from '../cell-types.store';
import { StructureAction } from '../models/structure-action.model';
import { StructureNode } from '../models/structure-node';
import { PanelNavigator } from '../panel-navigator';
import { StructureActions } from '../structure-actions';
import { StructureIcons } from '../structure-icons';
import { StructureMenu } from '../structure-menu';
import { COLUMN_BLOCKS_KEY, StructureTreeUtil } from '../structure-tree.util';
import { TemplatesStore } from '../templates.store';

/**
 * The table's whole structure as a tree: the table, header, sections, sub-sections, rows and cells,
 * and a horizontal table's column blocks, each with its level's icon and a short summary. Selecting a
 * node opens its settings, and selecting anything elsewhere reveals and highlights its node. Pointing
 * at a node shows "+" (add its natural child) and "⋯" (its menu, also on right-click). Sub-sections,
 * rows and column blocks drag to a new place among their siblings, or move with Alt+↑ and Alt+↓.
 */
@Component({
  selector: 'app-structure-outline',
  imports: [ButtonModule, TooltipModule, TreeModule],
  // The tree's drag and drop needs this service from its host.
  providers: [TreeDragDropService],
  templateUrl: './structure-outline.html',
  styleUrl: './structure-outline.scss',
})
export class StructureOutline {
  public readonly nodes = computed<StructureNode[]>(() => {
    // Rebuilt after a drop, which moves nodes in place before the save comes back.
    this.rebuild();
    const template = this.store.template();
    return template
      ? StructureTreeUtil.build(
          template,
          this.store.index(),
          (id) => this.cellTypes.cellType(id),
          this.collapsedKeys(),
        )
      : [];
  });

  /** Rows start folded; what the user opens or folds, and what selection reveals, overrides that. */
  public readonly collapsedKeys = computed<Record<string, boolean>>(() => {
    const merged = {
      ...StructureTreeUtil.defaultCollapsedKeys(this.store.index()),
      ...this.toggled(),
    };
    return Object.fromEntries(Object.entries(merged).filter(([, collapsed]) => collapsed));
  });

  /** The node for what the panel is showing, as the same object the tree renders. */
  public readonly selected = computed<StructureNode | null>(() => {
    const key = StructureTreeUtil.keyFor(this.navigator.current());
    return key === null ? null : (this.byKey().get(key) ?? null);
  });

  /** Each node's "+" action, worked out once per change rather than on every check. */
  public readonly addActions = computed<ReadonlyMap<string, StructureAction>>(() => {
    const actions = new Map<string, StructureAction>();
    for (const node of this.byKey().values()) {
      const ref = node.data?.ref;
      const action =
        node.key === COLUMN_BLOCKS_KEY
          ? this.actions.addColumnBlock()
          : ref
            ? this.actions.addChildFor(ref)
            : null;
      if (action && node.key) {
        actions.set(node.key, action);
      }
    }
    return actions;
  });

  public readonly canEdit = computed(() => this.store.canEdit());

  private readonly store = inject(TemplatesStore);
  private readonly cellTypes = inject(CellTypesStore);
  private readonly navigator = inject(PanelNavigator);
  private readonly actions = inject(StructureActions);
  private readonly menu = inject(StructureMenu);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** Nodes the user opened (false) or folded (true), and nodes opened to reveal the selection. */
  private readonly toggled = signal<Record<string, boolean>>({});
  private readonly rebuild = signal(0);

  private readonly byKey = computed<ReadonlyMap<string, StructureNode>>(() => {
    const map = new Map<string, StructureNode>();
    StructureOutline.collect(this.nodes(), map);
    return map;
  });

  private readonly parents = computed<ReadonlyMap<string, StructureNode>>(() => {
    const map = new Map<string, StructureNode>();
    for (const node of this.byKey().values()) {
      for (const child of node.children ?? []) {
        if (child.key) {
          map.set(child.key, node);
        }
      }
    }
    return map;
  });

  constructor() {
    // Selecting something in the preview or the panel opens the nodes above it.
    effect(() => {
      const keys = StructureTreeUtil.ancestorKeys(this.navigator.current(), this.store.index());
      untracked(() => {
        const toggled = this.toggled();
        if (keys.some((key) => toggled[key] !== false)) {
          this.toggled.set({ ...toggled, ...Object.fromEntries(keys.map((key) => [key, false])) });
        }
      });
    });

    // ...and brings its node into view.
    afterRenderEffect(() => {
      if (this.selected()) {
        this.host.nativeElement
          .querySelector('.p-tree-node-selected')
          ?.scrollIntoView({ block: 'nearest' });
      }
    });
  }

  /** Adds every node under `nodes`, at any depth, to `map` by key. */
  private static collect(nodes: StructureNode[], map: Map<string, StructureNode>): void {
    for (const node of nodes) {
      if (node.key) {
        map.set(node.key, node);
      }
      StructureOutline.collect(node.children ?? [], map);
    }
  }

  public icon(node: StructureNode): string {
    const data = node.data;
    return data ? `pi ${data.icon} ${StructureIcons.levelClass(data.level)}` : '';
  }

  public addAction(node: StructureNode): StructureAction | undefined {
    return node.key ? this.addActions().get(node.key) : undefined;
  }

  public open(event: TreeNodeSelectEvent): void {
    const ref = (event.node as StructureNode).data?.ref;
    if (ref) {
      this.navigator.open(ref);
    }
  }

  public expanded(event: TreeNodeExpandEvent): void {
    this.setToggled(event.node as StructureNode, false);
  }

  public collapsed(event: TreeNodeCollapseEvent): void {
    this.setToggled(event.node as StructureNode, true);
  }

  public add(action: StructureAction, event: Event): void {
    event.stopPropagation();
    action.run(event.currentTarget);
  }

  public openMenu(event: MouseEvent, node: StructureNode): void {
    const ref = node.data?.ref;
    if (ref) {
      this.menu.open(event, ref);
    } else {
      event.preventDefault();
    }
  }

  /** Alt+↑ and Alt+↓ move the selected sub-section, row or column block. */
  public keydown(event: KeyboardEvent): void {
    if (!event.altKey || (event.key !== 'ArrowUp' && event.key !== 'ArrowDown')) {
      return;
    }
    event.preventDefault();
    event.stopPropagation();
    this.actions.move(this.navigator.current(), event.key === 'ArrowUp' ? -1 : 1);
  }

  /**
   * A drop between two nodes of the same kind under the same parent moves the dragged node there.
   * Anything else (onto a node, or into another section) is refused, since parts can't change parent.
   */
  public drop(event: TreeNodeDropEvent): void {
    const dragged = event.dragNode as StructureNode | null | undefined;
    const target = event.dropNode as StructureNode | null | undefined;
    const ref = dragged?.data?.ref;
    if (
      event.dropPoint !== 'between' ||
      !ref ||
      !dragged?.key ||
      !target?.key ||
      dragged.data?.level !== target.data?.level
    ) {
      return;
    }
    const parent = this.parents().get(dragged.key);
    if (!parent || parent !== this.parents().get(target.key)) {
      return;
    }

    // Let the tree place the node, then read its new place among its own kind.
    event.accept?.();
    const position =
      (parent.children ?? [])
        .filter((node) => node.data?.level === dragged.data?.level)
        .indexOf(dragged) + 1;
    void this.actions
      .moveTo(ref, position)
      .finally(() => this.rebuild.update((count) => count + 1));
  }

  private setToggled(node: StructureNode, collapsed: boolean): void {
    if (node.key) {
      const key = node.key;
      this.toggled.update((toggled) => ({ ...toggled, [key]: collapsed }));
    }
  }
}
