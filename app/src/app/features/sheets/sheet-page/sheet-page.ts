import { Component, computed, inject, input } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { Phase } from '../../../core/models/phase.model';
import { SheetType } from '../../../core/models/sheet-type.model';
import { Breadcrumb } from '../../../shared/components/breadcrumb/breadcrumb';
import { BreadcrumbItem } from '../../../shared/components/breadcrumb/breadcrumb-item.model';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { PhasesStore } from '../../phases/phases.store';
import { SheetPlaceholder } from '../sheet-placeholder/sheet-placeholder';
import { SheetSwitcher } from '../sheet-switcher/sheet-switcher';
import { SheetVersionPicker } from '../sheet-version-picker/sheet-version-picker';

/**
 * One PU Spec Sheet for a phase (`/phases/:phaseId/sheets/:sheetTypeId`), full width. Each sheet is
 * versioned on its own; the version picker comes alive once versions exist.
 */
@Component({
  selector: 'app-sheet-page',
  imports: [
    Breadcrumb,
    ButtonModule,
    EmptyState,
    SheetPlaceholder,
    SheetSwitcher,
    SheetVersionPicker,
  ],
  providers: [PhasesStore],
  templateUrl: './sheet-page.html',
  styleUrl: './sheet-page.scss',
})
export class SheetPage {
  /** Bound from the route parameters. */
  public readonly phaseId = input.required<string>();
  public readonly sheetTypeId = input.required<string>();

  public readonly phase = computed<Phase | undefined>(() =>
    this.store.phaseById(Number(this.phaseId())),
  );

  /** The phase's available sheets, for the switcher. */
  public readonly phaseSheetTypes = computed<SheetType[]>(() => {
    const phase = this.phase();
    return phase ? this.store.sheetTypesFor(phase) : [];
  });

  /** The open sheet type, only if it's available to this phase. */
  public readonly sheetType = computed<SheetType | undefined>(() =>
    this.phaseSheetTypes().find((sheetType) => sheetType.id === Number(this.sheetTypeId())),
  );

  /** Phases › V6 › 01-A2 › Specification. */
  public readonly breadcrumb = computed<BreadcrumbItem[]>(() => {
    const phase = this.phase();
    if (!phase) {
      return [];
    }
    return [
      { label: 'Phases', link: ['/phases'] },
      ...this.store
        .ancestorsOf(phase.id)
        .map((ancestor) => ({ label: ancestor.code, link: ['/phases', ancestor.id] })),
      { label: phase.code, link: ['/phases', phase.id] },
      { label: this.sheetType()?.name ?? '' },
    ];
  });

  private readonly store = inject(PhasesStore);

  public isLoading(): boolean {
    return this.store.isLoading();
  }

  public hasError(): boolean {
    return this.store.hasError();
  }

  public retry(): void {
    this.store.reload();
  }
}
