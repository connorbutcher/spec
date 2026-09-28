import { httpResource } from '@angular/common/http';
import { computed, inject, Injectable } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRouteSnapshot, NavigationEnd, Router } from '@angular/router';
import { TreeNode } from 'primeng/api';
import { filter } from 'rxjs';
import { Phase } from '../../core/models/phase.model';
import { SheetType } from '../../core/models/sheet-type.model';
import { buildPhaseTree, indexTreeNodes, phaseAncestors } from './phase-tree.util';

/**
 * State for the phases screen: the phase list and sheet types (fetched with resources), the tree
 * built from them and which phase is selected. Provided by the phases page so the tree and the detail
 * panel share one instance.
 */
@Injectable()
export class PhasesStore {
  public readonly isLoading = computed(
    () => this.phasesResource.isLoading() || this.sheetTypesResource.isLoading(),
  );

  public readonly hasError = computed(
    () => this.phasesResource.status() === 'error' || this.sheetTypesResource.status() === 'error',
  );

  public readonly phases = computed<Phase[]>(() =>
    this.phasesResource.hasValue() ? this.phasesResource.value() : [],
  );

  public readonly sheetTypes = computed<SheetType[]>(() =>
    this.sheetTypesResource.hasValue()
      ? [...this.sheetTypesResource.value()].sort((a, b) => a.displayOrder - b.displayOrder)
      : [],
  );

  public readonly tree = computed<TreeNode<Phase>[]>(() => buildPhaseTree(this.phases()));

  /** The phase id in the current URL (`/phases/:phaseId`), or null on `/phases`. */
  public readonly selectedPhaseId = computed<number | null>(() => {
    this.navigationEnd();
    const phaseId = findRouteParam(this.router.routerState.snapshot.root, 'phaseId');
    return phaseId === null ? null : Number(phaseId);
  });

  /** The tree node for the selected phase, as the same object the tree renders. */
  public readonly selectedTreeNode = computed<TreeNode<Phase> | null>(() => {
    const id = this.selectedPhaseId();
    return id === null ? null : (this.treeNodesById().get(id) ?? null);
  });

  private readonly router = inject(Router);

  private readonly navigationEnd = toSignal(
    this.router.events.pipe(filter((event) => event instanceof NavigationEnd)),
  );

  private readonly phasesResource = httpResource<Phase[]>(() => '/api/phases');

  private readonly sheetTypesResource = httpResource<SheetType[]>(() => '/api/sheet-types');

  private readonly phasesById = computed(
    () => new Map(this.phases().map((phase) => [phase.id, phase])),
  );

  private readonly treeNodesById = computed(() => indexTreeNodes(this.tree()));

  public reload(): void {
    this.phasesResource.reload();
    this.sheetTypesResource.reload();
  }

  public selectPhase(id: number): void {
    void this.router.navigate(['/phases', id]);
  }

  public phaseById(id: number): Phase | undefined {
    return this.phasesById().get(id);
  }

  public ancestorsOf(id: number): Phase[] {
    return phaseAncestors(id, this.phasesById());
  }

  /** The sheet types selected for a phase, in sheet type display order. */
  public sheetTypesFor(phase: Phase): SheetType[] {
    const selected = new Set(phase.sheetTypeIds);
    return this.sheetTypes().filter((sheetType) => selected.has(sheetType.id));
  }
}

function findRouteParam(route: ActivatedRouteSnapshot, name: string): string | null {
  let current: ActivatedRouteSnapshot | null = route;
  while (current) {
    const value = current.paramMap.get(name);
    if (value !== null) {
      return value;
    }
    current = current.firstChild;
  }
  return null;
}
