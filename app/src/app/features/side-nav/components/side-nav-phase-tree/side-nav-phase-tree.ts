import { Component, inject, Signal } from '@angular/core';
import { TreeNode } from 'primeng/api';
import { Skeleton } from 'primeng/skeleton';
import { Tree, TreeNodeSelectEvent } from 'primeng/tree';
import { SpecSheetPhase } from '../../../../core/models/domain/types/spec-sheet-phase';
import { PhasesService } from '../../../../core/services/phases.service';

@Component({
  selector: 'app-side-nav-phase-tree',
  imports: [Tree, Skeleton],
  templateUrl: './side-nav-phase-tree.html',
  styleUrl: './side-nav-phase-tree.scss',
})
export class SideNavPhaseTree {
  private readonly _phasesService = inject(PhasesService);

  public onNodeSelect(event: TreeNodeSelectEvent): void {
    const phase = (event.node as TreeNode<SpecSheetPhase>).data;
    if (phase) {
      this._phasesService.selectPhase(phase.id);
    }
  }

  public onNodeUnselect(event: TreeNodeSelectEvent): void {
    this._phasesService.deselectPhase();
  }

  public get nodes(): Signal<TreeNode<SpecSheetPhase>[]> {
    return this._phasesService.phasesTree;
  }

  public get selectedPhase(): Signal<TreeNode<SpecSheetPhase> | null> {
    return this._phasesService.selectedPhaseNode;
  }

  public get isLoading(): Signal<boolean> {
    return this._phasesService.isLoading;
  }

  public get hasError(): Signal<boolean> {
    return this._phasesService.hasError;
  }
}
