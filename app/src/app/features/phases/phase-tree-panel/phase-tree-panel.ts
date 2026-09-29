import { Component, inject } from '@angular/core';
import { TreeNode } from 'primeng/api';
import { BadgeModule } from 'primeng/badge';
import { ButtonModule } from 'primeng/button';
import { TreeModule, TreeNodeSelectEvent } from 'primeng/tree';
import { Phase } from '../../../core/models/phase.model';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { PhaseTreeSkeleton } from '../phase-tree-skeleton/phase-tree-skeleton';
import { PhasesStore } from '../phases.store';

/** The left-hand card: the PrimeNG phase tree with its filter, plus loading, error and empty states. */
@Component({
  selector: 'app-phase-tree-panel',
  imports: [BadgeModule, ButtonModule, EmptyState, PhaseTreeSkeleton, TreeModule],
  templateUrl: './phase-tree-panel.html',
  styleUrl: './phase-tree-panel.scss',
})
export class PhaseTreePanel {
  private readonly store = inject(PhasesStore);

  public isLoading(): boolean {
    return this.store.isLoading();
  }

  public hasError(): boolean {
    return this.store.hasError();
  }

  public hasPhases(): boolean {
    return this.store.phases().length > 0;
  }

  public nodes(): TreeNode<Phase>[] {
    return this.store.tree();
  }

  public selectedNode(): TreeNode<Phase> | null {
    return this.store.selectedTreeNode();
  }

  public onNodeSelect(event: TreeNodeSelectEvent): void {
    const phase = (event.node as TreeNode<Phase>).data;
    if (phase) {
      this.store.selectPhase(phase.id);
    }
  }

  public sheetTypeCount(node: TreeNode<Phase>): number {
    return node.data?.sheetTypeIds.length ?? 0;
  }

  public retry(): void {
    this.store.reload();
  }
}
