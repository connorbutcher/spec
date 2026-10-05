import { Component, computed, inject, input, output } from '@angular/core';
import { TreeNode } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { TreeModule, TreeNodeSelectEvent } from 'primeng/tree';
import { Phase } from '../../../core/models/phase.model';
import { buildPhaseTree, indexTreeNodes } from '../../phases/phase-tree.util';
import { PhasesStore } from '../../phases/phases.store';

/** The phase tree of the admin screen, with the button that adds a phase. */
@Component({
  selector: 'app-admin-phase-tree',
  imports: [ButtonModule, TooltipModule, TreeModule],
  templateUrl: './admin-phase-tree.html',
  styleUrl: './admin-phase-tree.scss',
})
export class AdminPhaseTree {
  public readonly selectedPhaseId = input<number | null>(null);
  public readonly canManage = input(false);

  public readonly selected = output<number>();
  public readonly add = output<void>();

  /** This screen's own nodes, so expanding and collapsing here leaves the side nav's tree alone. */
  public readonly tree = computed<TreeNode<Phase>[]>(() => buildPhaseTree(this.store.phases()));

  /** The node for the picked phase, as the same object the tree renders. */
  public readonly selectedNode = computed<TreeNode<Phase> | null>(() => {
    const id = this.selectedPhaseId();
    return id === null ? null : (this.nodesById().get(id) ?? null);
  });

  private readonly store = inject(PhasesStore);

  private readonly nodesById = computed(() => indexTreeNodes(this.tree()));

  public isLoading(): boolean {
    return this.store.isLoading() && this.store.phases().length === 0;
  }

  public hasError(): boolean {
    return this.store.hasError();
  }

  public retry(): void {
    this.store.reload();
  }

  public onNodeSelect(event: TreeNodeSelectEvent): void {
    const phase = (event.node as TreeNode<Phase>).data;
    if (phase) {
      this.selected.emit(phase.id);
    }
  }
}
