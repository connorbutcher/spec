import { Component, computed, input, linkedSignal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { PhaseTreeNode } from '../../../core/models/phase-tree-node.model';

/** One phase in the tree, with its children nested beneath it. Renders itself recursively. */
@Component({
  selector: 'app-phase-tree-node',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './phase-tree-node.component.html',
  styleUrl: './phase-tree-node.component.scss',
  host: { role: 'none' },
})
export class PhaseTreeNodeComponent {
  public readonly node = input.required<PhaseTreeNode>();
  public readonly depth = input(0);
  public readonly forceExpanded = input(false);

  public readonly hasChildren = computed(() => this.node().children.length > 0);
  public readonly sheetTypeCount = computed(() => this.node().phase.sheetTypeIds.length);
  public readonly indent = computed(() => `${this.depth() * 16 + 4}px`);

  /** Open by default, and reopened whenever filtering starts or stops. The user can still toggle it. */
  public readonly expanded = linkedSignal({ source: this.forceExpanded, computation: () => true });

  public toggle(): void {
    this.expanded.update((expanded) => !expanded);
  }
}
