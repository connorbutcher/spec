import { Component, computed, inject, input } from '@angular/core';
import { Phase } from '../../../core/models/phase.model';
import { SheetType } from '../../../core/models/sheet-type.model';
import { Breadcrumb } from '../../../shared/components/breadcrumb/breadcrumb';
import { BreadcrumbItem } from '../../../shared/components/breadcrumb/breadcrumb-item.model';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { PhaseDetailHeader } from '../phase-detail-header/phase-detail-header';
import { PhasesStore } from '../phases.store';
import { SheetTypeTable } from '../sheet-type-table/sheet-type-table';

/** The right-hand card for the phase selected in the tree (from the `:phaseId` route parameter). */
@Component({
  selector: 'app-phase-detail',
  imports: [Breadcrumb, EmptyState, PhaseDetailHeader, SheetTypeTable],
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

  /** Phases › V6 › 01-A2. */
  public readonly breadcrumb = computed<BreadcrumbItem[]>(() => [
    { label: 'Phases', link: ['/phases'] },
    ...this.ancestors().map((ancestor) => ({
      label: ancestor.code,
      link: ['/phases', ancestor.id],
    })),
    { label: this.phase()?.code ?? '' },
  ]);

  public readonly sheetTypes = computed<SheetType[]>(() => {
    const phase = this.phase();
    return phase ? this.store.sheetTypesFor(phase) : [];
  });

  private readonly store = inject(PhasesStore);

  public isLoading(): boolean {
    return this.store.isLoading();
  }

  /** When loading failed the tree panel shows the error, so this panel shouldn't claim "not found". */
  public hasError(): boolean {
    return this.store.hasError();
  }
}
