import { httpResource } from '@angular/common/http';
import { computed, Injectable, signal } from '@angular/core';
import { Phase } from '../../core/models/phase.model';
import { SheetType } from '../../core/models/sheet-type.model';
import { buildPhaseTree, comparePhases, filterPhaseTree, phaseAncestors } from './phase-tree.util';

/**
 * State for the phases screen: the phase list and sheet types (fetched with resources), the tree
 * built from them and the tree filter. Provided by the phases page so the tree and the detail panel
 * share one instance.
 */
@Injectable()
export class PhasesStore {
  public readonly filterQuery = signal('');

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

  public readonly tree = computed(() => buildPhaseTree(this.phases()));

  public readonly visibleTree = computed(() => filterPhaseTree(this.tree(), this.filterQuery()));

  public readonly isFiltering = computed(() => this.filterQuery().trim().length > 0);

  private readonly phasesResource = httpResource<Phase[]>(() => '/api/phases');

  private readonly sheetTypesResource = httpResource<SheetType[]>(() => '/api/sheet-types');

  private readonly phasesById = computed(
    () => new Map(this.phases().map((phase) => [phase.id, phase])),
  );

  public reload(): void {
    this.phasesResource.reload();
    this.sheetTypesResource.reload();
  }

  public phaseById(id: number): Phase | undefined {
    return this.phasesById().get(id);
  }

  public ancestorsOf(id: number): Phase[] {
    return phaseAncestors(id, this.phasesById());
  }

  public childrenOf(id: number): Phase[] {
    return this.phases()
      .filter((phase) => phase.parentPhaseId === id)
      .sort(comparePhases);
  }

  /** The sheet types selected for a phase, in sheet type display order. */
  public sheetTypesFor(phase: Phase): SheetType[] {
    const selected = new Set(phase.sheetTypeIds);
    return this.sheetTypes().filter((sheetType) => selected.has(sheetType.id));
  }
}
