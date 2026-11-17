import { httpResource } from '@angular/common/http';
import { computed, inject, Injectable } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { TreeNode } from 'primeng/api';
import { filter } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { RouteParamUtil } from '../helpers/route-param.util';
import { SpecSheetPhaseHelpers } from '../helpers/spec-sheet-phase-helpers';
import { SpecSheetPhase } from '../models/domain/types/spec-sheet-phase';

@Injectable({
  providedIn: 'root',
})
export class PhasesService {
  public readonly isLoading = computed(() =>
    this._specSheetPhasesResource.isLoading(),
  );
  public readonly hasError = computed(
    () => !!this._specSheetPhasesResource.error(),
  );

  public readonly specSheetPhases = computed<SpecSheetPhase[]>(
    () => this._specSheetPhasesResource.value() ?? [],
  );
  public readonly flatPhases = computed(() =>
    this.specSheetPhases().flatMap((p) => [p, ...p.phases]),
  );
  public readonly phasesTree = computed<TreeNode<SpecSheetPhase>[]>(() => {
    const phases = this.specSheetPhases();
    return phases.map((phase) => ({
      data: phase,
      key: `${phase.id}`,
      label: phase.phase,
      expanded:
        phase.id === this.selectedPhaseId() ||
        phase.phases.some((subPhase) => this.selectedPhaseId() === subPhase.id),
      selected: this.selectedPhaseId() === phase.id,
      children: phase.phases.map((subPhase) => ({
        data: subPhase,
        key: `${subPhase.id}`,
        label: subPhase.phase,
        expanded: this.selectedPhaseId() === subPhase.id,
        selected: this.selectedPhaseId() === subPhase.id,
        children: [],
      })),
    }));
  });

  public readonly selectedPhaseId = computed<number | null>(() => {
    this._navigationEnd();

    const phaseId = RouteParamUtil.findRouteParam(
      this._router.routerState.snapshot.root,
      'phaseId',
    );
    if (phaseId === null || isNaN(Number(phaseId))) {
      return null;
    }

    return Number(phaseId);
  });

  public readonly selectedPhaseNode = computed<TreeNode<SpecSheetPhase> | null>(
    () => {
      const phaseId = this.selectedPhaseId();
      return phaseId === null
        ? null
        : (this._treeNodesById().get(phaseId) ?? null);
    },
  );
  public readonly selectedPhase = computed<SpecSheetPhase | null>(() => {
    const node = this.selectedPhaseNode();
    return node ? node.data! : null;
  });

  private readonly _specSheetPhasesResource = httpResource<SpecSheetPhase[]>(
    () => `${environment.apiUrl}/specsheetphases/all`,
  );

  private readonly _router = inject(Router);
  private readonly _activatedRoute = inject(ActivatedRoute);

  private readonly _navigationEnd = toSignal(
    this._router.events.pipe(filter((event) => event instanceof NavigationEnd)),
  );
  private readonly _treeNodesById = computed(() =>
    SpecSheetPhaseHelpers.indexTreeNodes(this.phasesTree()),
  );

  public selectPhase(id: number): void {
    this._router.navigate(['phases', id], {
      queryParamsHandling: 'replace',
    });
  }

  public deselectPhase(): void {
    this._router.navigate(['phases'], {
      queryParamsHandling: 'replace',
    });
  }

  public refresh(): void {
    this._specSheetPhasesResource.reload();
  }
}
