import { Component, computed, inject, input, output, signal } from '@angular/core';
import { TreeDragDropService, TreeNode } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { TooltipModule } from 'primeng/tooltip';
import { TreeModule, TreeNodeDropEvent, TreeNodeSelectEvent } from 'primeng/tree';
import { Phase } from '../../../core/models/phase.model';
import { buildPhaseTree, indexTreeNodes } from '../../phases/phase-tree.util';
import { PhasesStore } from '../../phases/phases.store';
import { apiErrorMessage } from '../../templates/api-error-message';
import { AdminApi } from '../admin-api';
import { phaseLocation } from '../admin.util';

/**
 * The phase tree of the admin screen, with the button that adds a phase. With permission, a phase can
 * be dragged to a new place among its siblings, onto another phase to go under it, or to the top level.
 */
@Component({
  selector: 'app-admin-phase-tree',
  imports: [ButtonModule, MessageModule, TooltipModule, TreeModule],
  providers: [TreeDragDropService],
  templateUrl: './admin-phase-tree.html',
  styleUrl: './admin-phase-tree.scss',
})
export class AdminPhaseTree {
  public readonly selectedPhaseId = input<number | null>(null);
  public readonly canManage = input(false);

  public readonly selected = output<number>();
  public readonly add = output<void>();

  public readonly isMoving = signal(false);
  public readonly error = signal<string | null>(null);

  /** This screen's own nodes, so dragging, expanding and collapsing here leave the side nav's tree alone. */
  public readonly tree = computed<TreeNode<Phase>[]>(() => buildPhaseTree(this.store.phases()));

  /** The node for the picked phase, as the same object the tree renders. */
  public readonly selectedNode = computed<TreeNode<Phase> | null>(() => {
    const id = this.selectedPhaseId();
    return id === null ? null : (this.nodesById().get(id) ?? null);
  });

  private readonly store = inject(PhasesStore);
  private readonly api = inject(AdminApi);

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

  /**
   * Saves where a dragged phase was dropped. The tree only says which node it was dropped on, so the
   * drop is accepted first and the phase's new place is read from the nodes. The phases are then loaded
   * again either way: to show the saved order, or to put the phase back when the move was refused.
   */
  public async onNodeDrop(event: TreeNodeDropEvent): Promise<void> {
    const phase = (event.dragNode as TreeNode<Phase> | null | undefined)?.data;
    if (!phase || !event.accept || this.isMoving()) {
      return;
    }

    const before = phaseLocation(this.tree(), phase.id);
    event.accept();
    const after = phaseLocation(this.tree(), phase.id);
    if (
      !after ||
      (before?.parentPhaseId === after.parentPhaseId && before.position === after.position)
    ) {
      return;
    }

    this.isMoving.set(true);
    this.error.set(null);
    try {
      await this.api.movePhase(phase.id, after.parentPhaseId, after.position);
    } catch (error) {
      this.error.set(apiErrorMessage(error));
    } finally {
      this.store.reload();
      this.isMoving.set(false);
    }
  }
}
