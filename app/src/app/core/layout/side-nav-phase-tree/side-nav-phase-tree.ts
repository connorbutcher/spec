import { Component, computed, inject, input } from '@angular/core';
import { MenuItem, TreeNode } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { TreeModule, TreeNodeSelectEvent } from 'primeng/tree';
import { PhaseTreeSkeleton } from '../../../features/phases/phase-tree-skeleton/phase-tree-skeleton';
import { PhasesStore } from '../../../features/phases/phases.store';
import { Phase } from '../../models/phase.model';

/**
 * The phase tree inside the side nav, under the Phases entry: a PrimeNG tree on the dark rail. Picking
 * a phase opens it, or, when a sheet is open and the phase has that sheet type, opens the same sheet for
 * that phase. When the nav is collapsed the phases shrink to a column of codes.
 */
@Component({
  selector: 'app-side-nav-phase-tree',
  imports: [ButtonModule, MenuModule, PhaseTreeSkeleton, TreeModule],
  templateUrl: './side-nav-phase-tree.html',
  styleUrl: './side-nav-phase-tree.scss',
})
export class SideNavPhaseTree {
  public readonly store = inject(PhasesStore);

  public readonly collapsed = input(false);

  /** Every phase as a flat list in tree order, for the collapsed rail. */
  public readonly codes = computed<MenuItem[]>(() => {
    const items: MenuItem[] = [];
    const visit = (nodes: TreeNode<Phase>[]): void => {
      for (const node of nodes) {
        items.push({
          label: node.label,
          styleClass: node.data?.id === this.store.selectedPhaseId() ? 'selected' : '',
          command: () => {
            if (node.data) {
              this.store.selectPhase(node.data.id);
            }
          },
        });
        visit(node.children ?? []);
      }
    };
    visit(this.store.tree());
    return items;
  });

  public onNodeSelect(event: TreeNodeSelectEvent): void {
    const phase = (event.node as TreeNode<Phase>).data;
    if (phase) {
      this.store.selectPhase(phase.id);
    }
  }
}
