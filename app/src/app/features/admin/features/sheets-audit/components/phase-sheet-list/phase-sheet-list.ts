import { Component, computed, effect, inject, model } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TreeNode } from 'primeng/api';
import { InputGroup } from 'primeng/inputgroup';
import { InputGroupAddon } from 'primeng/inputgroupaddon';
import { InputText } from 'primeng/inputtext';
import { Tag } from 'primeng/tag';
import { TreeModule } from 'primeng/tree';
import { SpecSheetPhase } from '../../../../../../core/models/domain/types/spec-sheet-phase';
import { PhasesService } from '../../../../../../core/services/phases.service';
import { SheetsAuditService } from '../../sheets-audit.service';

@Component({
  selector: 'app-phase-sheet-list',
  imports: [
    FormsModule,
    TreeModule,
    InputGroup,
    InputGroupAddon,
    InputText,
    Tag,
  ],
  templateUrl: './phase-sheet-list.html',
  styleUrl: './phase-sheet-list.scss',
})
export class PhaseSheetList {
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
  public readonly activePhase = model<TreeNode<SpecSheetPhase> | undefined>(
    undefined,
  );

  private readonly _phasesService = inject(PhasesService);
  private readonly _phaseService = inject(SheetsAuditService);
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

  public selectPhase(phase: SpecSheetPhase): void {
    this._phaseService.activePhase.set(phase);
    this._phaseService.activeType.set(undefined);

    this._router.navigate([], {
      relativeTo: this._activatedRoute,
      queryParams: { phaseId: phase.id },
      queryParamsHandling: 'replace',
    });
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
