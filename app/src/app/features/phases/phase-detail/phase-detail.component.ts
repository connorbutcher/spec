import { Component, computed, inject, input } from '@angular/core';
import { Phase } from '../../../core/models/phase.model';
import { SheetType } from '../../../core/models/sheet-type.model';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { PhasesStore } from '../phases.store';
import { ChildPhaseListComponent } from './child-phase-list.component';
import { PhaseBreadcrumbComponent } from './phase-breadcrumb.component';
import { PhaseDetailHeaderComponent } from './phase-detail-header.component';
import { SheetTypeGridComponent } from './sheet-type-grid.component';

/** The right-hand panel for the phase selected in the tree (from the `:phaseId` route parameter). */
@Component({
  selector: 'app-phase-detail',
  imports: [
    ChildPhaseListComponent,
    EmptyStateComponent,
    PhaseBreadcrumbComponent,
    PhaseDetailHeaderComponent,
    SheetTypeGridComponent,
  ],
  templateUrl: './phase-detail.component.html',
  styleUrl: './phase-detail.component.scss',
})
export class PhaseDetailComponent {
  /** Bound from the route parameter. */
  public readonly phaseId = input.required<string>();

  public readonly phase = computed<Phase | undefined>(() =>
    this.store.phaseById(Number(this.phaseId())),
  );

  public readonly ancestors = computed<Phase[]>(() => {
    const phase = this.phase();
    return phase ? this.store.ancestorsOf(phase.id) : [];
  });

  public readonly children = computed<Phase[]>(() => {
    const phase = this.phase();
    return phase ? this.store.childrenOf(phase.id) : [];
  });

  public readonly sheetTypes = computed<SheetType[]>(() => {
    const phase = this.phase();
    return phase ? this.store.sheetTypesFor(phase) : [];
  });

  private readonly store = inject(PhasesStore);

  public isLoading(): boolean {
    return this.store.isLoading();
  }
}
