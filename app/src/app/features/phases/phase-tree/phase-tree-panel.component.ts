import { Component, inject } from '@angular/core';
import { PhaseTreeNode } from '../../../core/models/phase-tree-node.model';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { PhasesStore } from '../phases.store';
import { PhaseTreeFilterComponent } from './phase-tree-filter.component';
import { PhaseTreeSkeletonComponent } from './phase-tree-skeleton.component';
import { PhaseTreeComponent } from './phase-tree.component';

/** The left-hand panel: filter box plus the phase tree, with loading, error and empty states. */
@Component({
  selector: 'app-phase-tree-panel',
  imports: [
    EmptyStateComponent,
    PhaseTreeComponent,
    PhaseTreeFilterComponent,
    PhaseTreeSkeletonComponent,
  ],
  templateUrl: './phase-tree-panel.component.html',
  styleUrl: './phase-tree-panel.component.scss',
})
export class PhaseTreePanelComponent {
  /** Two-way bound to the filter box; the store derives the visible tree from it. */
  public readonly filterQuery = inject(PhasesStore).filterQuery;

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

  public nodes(): PhaseTreeNode[] {
    return this.store.visibleTree();
  }

  public isFiltering(): boolean {
    return this.store.isFiltering();
  }

  public retry(): void {
    this.store.reload();
  }
}
