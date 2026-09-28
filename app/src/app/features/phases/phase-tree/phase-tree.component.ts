import { Component, input } from '@angular/core';
import { PhaseTreeNode } from '../../../core/models/phase-tree-node.model';
import { PhaseTreeNodeComponent } from './phase-tree-node.component';

/** The top level of the phase tree. */
@Component({
  selector: 'app-phase-tree',
  imports: [PhaseTreeNodeComponent],
  template: `
    <ul class="phase-tree" role="tree" aria-label="Phases">
      @for (node of nodes(); track node.phase.id) {
        <app-phase-tree-node [node]="node" [depth]="0" [forceExpanded]="forceExpanded()" />
      }
    </ul>
  `,
  styles: `
    .phase-tree {
      margin: 0;
      padding: 0;
      list-style: none;
    }
  `,
})
export class PhaseTreeComponent {
  public readonly nodes = input.required<PhaseTreeNode[]>();
  /** Expands every node, e.g. while filtering so every match is visible. */
  public readonly forceExpanded = input(false);
}
