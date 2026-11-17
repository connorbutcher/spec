import {
  Component,
  computed,
  effect,
  inject,
  model,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MessageService, TreeNode } from 'primeng/api';
import { ButtonDirective } from 'primeng/button';
import { InputGroup } from 'primeng/inputgroup';
import { InputGroupAddon } from 'primeng/inputgroupaddon';
import { InputText } from 'primeng/inputtext';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';
import { TreeModule } from 'primeng/tree';
import { SpecSheetPhase } from '../../../../../../core/models/domain/types/spec-sheet-phase';
import { UpdateSpecSheetPhaseRequest } from '../../../../../../core/models/requests/types/phase/update-spec-sheet-phase-request';
import { PhasesService } from '../../../../../../core/services/phases.service';
import { PhaseDataService } from '../../phase-data.service';
import { PhaseService } from '../../phase.service';

@Component({
  selector: 'app-phase-list',
  imports: [
    FormsModule,
    TreeModule,
    ButtonDirective,
    InputGroup,
    InputGroupAddon,
    InputText,
    Tooltip,
    Tag,
  ],
  templateUrl: './phase-list.html',
  styleUrl: './phase-list.scss',
})
export class PhaseList {
  public readonly phaseOptions = computed<TreeNode[]>(() => {
    const phases = this._phasesService.specSheetPhases();
    const activePhase = this._phaseService.activePhase();

    const nodes: TreeNode[] = [];
    for (const phase of phases) {
      const children = phase.phases.map((subPhase) => ({
        key: `${subPhase.id}`,
        label: subPhase.phase,
        data: subPhase,
        type: 'phase',
        selectable: !subPhase.isObsolete,
        expanded: activePhase?.id === subPhase.id,
        draggable: !subPhase.isObsolete,
        droppable: !subPhase.isObsolete,
      }));

      const hasExpandedChild = children.some((child) => child.expanded);

      nodes.push({
        key: `${phase.id}`,
        label: phase.phase,
        data: phase,
        type: 'phase',
        selectable: !phase.isObsolete,
        expanded: hasExpandedChild || activePhase?.id === phase.id,
        draggable: !phase.isObsolete,
        droppable: !phase.isObsolete,
        children,
      });
    }

    return nodes;
  });
  public readonly hasChangedTree = computed(
    () => this._updateRequests().length > 0,
  );
  public readonly isSaving = signal(false);
  public readonly activePhase = model<TreeNode<SpecSheetPhase> | undefined>(
    undefined,
  );

  private readonly _updateRequests = signal<UpdateSpecSheetPhaseRequest[]>([]);
  private readonly _baselineMap = computed(() =>
    this.buildBaselineMap(this._phasesService.specSheetPhases()),
  );

  private readonly _phasesService = inject(PhasesService);
  private readonly _phaseDataService = inject(PhaseDataService);
  private readonly _phaseService = inject(PhaseService);
  private readonly _messageService = inject(MessageService);
  private readonly _router = inject(Router);
  private readonly _activatedRoute = inject(ActivatedRoute);

  constructor() {
    effect(() => {
      const activePhase = this._phaseService.activePhase();
      if (!activePhase) {
        this.activePhase.set(undefined);
        return;
      }

      const node = this.getTreeNode(this.phaseOptions(), activePhase.id);
      this.activePhase.set(node);
    });
  }

  public treeChanged(): void {
    const baseline = this._baselineMap();
    const updatedNodes = this.phaseOptions();
    const updateRequests = this.buildUpdateRequestsFromTree(
      updatedNodes,
      baseline,
    );

    this._updateRequests.set(updateRequests);
  }

  public async saveOrderChangesAsync(): Promise<void> {
    if (this.isSaving()) {
      return;
    }

    this.isSaving.set(true);

    try {
      const updateRequests = this._updateRequests();
      if (updateRequests.length === 0) {
        this.isSaving.set(false);
        return;
      }

      await this._phaseDataService.updateSpecSheetPhasesAsync(updateRequests);
      this._phasesService.refresh();
      this._updateRequests.set([]);
      this._messageService.add({
        severity: 'success',
        summary: 'Save Successful',
        detail: 'Phase order has been successfully updated.',
      });
    } catch (error) {
      this._messageService.add({
        severity: 'error',
        summary: 'Save Failed',
        detail: 'An error occurred while saving the phase order.',
      });
    } finally {
      this.isSaving.set(false);
    }
  }

  public addPhase(event: Event, parentPhase?: SpecSheetPhase): void {
    event.stopPropagation();

    this._router.navigate(['create'], {
      relativeTo: this._activatedRoute,
      queryParams: parentPhase ? { parentId: parentPhase.id } : {},
    });
    this._phaseService.activePhase.set(undefined);
  }

  public updatePhase(phase: SpecSheetPhase): void {
    this._router.navigate([phase.id], { relativeTo: this._activatedRoute });
  }

  private buildBaselineMap(
    phases: SpecSheetPhase[],
  ): Map<number, { parentId?: number; displayOrder: number }> {
    const map = new Map<number, { parentId?: number; displayOrder: number }>();

    const walk = (list: SpecSheetPhase[], parentId?: number): void => {
      for (let i = 0; i < list.length; i++) {
        const phase = list[i];
        map.set(phase.id, {
          parentId: parentId ?? phase.parentSpecSheetPhaseId ?? undefined,
          displayOrder: phase.displayOrder,
        });

        if (phase.phases?.length) {
          walk(phase.phases, phase.id);
        }
      }
    };

    walk(phases);
    return map;
  }

  private buildUpdateRequestsFromTree(
    nodes: TreeNode[],
    baseline: Map<number, { parentId?: number; displayOrder: number }>,
    parentId?: number,
  ): UpdateSpecSheetPhaseRequest[] {
    const updates: UpdateSpecSheetPhaseRequest[] = [];

    const walk = (list: TreeNode[], currentParentId?: number): void => {
      for (let i = 0; i < list.length; i++) {
        const node = list[i];
        const data = node.data as SpecSheetPhase;
        const newParentId = currentParentId;
        const newDisplayOrder = i + 1;
        const baselineEntry = baseline.get(data.id);

        const parentChanged = baselineEntry?.parentId !== newParentId;
        const orderChanged = baselineEntry?.displayOrder !== newDisplayOrder;

        if (parentChanged || orderChanged || !baselineEntry) {
          updates.push({
            id: data.id,
            phase: data.phase,
            phaseShort: data.phaseShort,
            isObsolete: data.isObsolete,
            parentSpecSheetPhaseId: newParentId,
            newSpecSheetTypeIds: [],
            removedSpecSheetTypeIds: [],
            displayOrder: newDisplayOrder,
            createSpecSheetPhaseObserverRequests: [],
            updateSpecSheetPhaseObserverRequests: [],
            removeSpecSheetPhaseObserverIds: [],
          });
        }

        if (node.children?.length) {
          walk(node.children, data.id);
        }
      }
    };

    walk(nodes, parentId);
    return updates;
  }

  private getTreeNode(
    nodes: TreeNode[],
    phaseId: number | null,
  ): TreeNode | undefined {
    if (!phaseId || nodes.length === 0) {
      return undefined;
    }

    const match = (node: TreeNode): TreeNode | undefined => {
      const data = node.data as SpecSheetPhase | undefined;
      if (data?.id === phaseId) {
        return node;
      }

      if (node.children?.length) {
        for (const child of node.children) {
          const matched = match(child);

          if (matched) {
            return matched;
          }
        }
      }

      node.expanded = false;

      return undefined;
    };

    for (const node of nodes) {
      const matched = match(node);
      if (matched) {
        return matched;
      }
    }

    return undefined;
  }
}
