import { Component, computed, inject, input } from '@angular/core';
import { Phase } from '../../../core/models/phase.model';
import { SheetType } from '../../../core/models/sheet-type.model';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { PhaseBreadcrumb } from '../phase-breadcrumb/phase-breadcrumb';
import { PhaseDetailHeader } from '../phase-detail-header/phase-detail-header';
import { PhasesStore } from '../phases.store';
import { SheetTypeList } from '../sheet-type-list/sheet-type-list';

/** The right-hand card for the phase selected in the tree (from the `:phaseId` route parameter). */
@Component({
  selector: 'app-phase-detail',
  imports: [EmptyState, PhaseBreadcrumb, PhaseDetailHeader, SheetTypeList],
  templateUrl: './phase-detail.html',
  styleUrl: './phase-detail.scss',
})
export class PhaseDetail {
  /** Bound from the route parameter. */
  public readonly phaseId = input.required<string>();

  public readonly phase = computed<Phase | undefined>(() =>
    this.store.phaseById(Number(this.phaseId())),
  );

  public readonly ancestors = computed<Phase[]>(() => {
    const phase = this.phase();
    return phase ? this.store.ancestorsOf(phase.id) : [];
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
