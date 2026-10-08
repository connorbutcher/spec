import { CdkDrag, CdkDragDrop, CdkDropList } from '@angular/cdk/drag-drop';
import { Component, computed, inject, input } from '@angular/core';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ConfirmPopupModule } from 'primeng/confirmpopup';
import { MessageModule } from 'primeng/message';
import { SkeletonModule } from 'primeng/skeleton';
import { ToastModule } from 'primeng/toast';
import { Phase } from '../../../core/models/phase.model';
import { SheetType } from '../../../core/models/sheet-type.model';
import { Breadcrumb } from '../../../shared/components/breadcrumb/breadcrumb';
import { BreadcrumbItem } from '../../../shared/components/breadcrumb/breadcrumb-item.model';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { PhasesStore } from '../../phases/phases.store';
import { RowCheckoutStore } from '../row-checkout.store';
import { RowTakeoverStore } from '../row-takeover.store';
import { SheetHub } from '../sheet-hub';
import { SheetLiveStore } from '../sheet-live.store';
import { SheetLoadError } from '../sheet-load-error/sheet-load-error';
import { SheetSwitcher } from '../sheet-switcher/sheet-switcher';
import { SheetTableCard } from '../sheet-table-card/sheet-table-card';
import { SheetTakeoverRequests } from '../sheet-takeover-requests/sheet-takeover-requests';
import { SheetToolbar } from '../sheet-toolbar/sheet-toolbar';
import { SheetStore } from '../sheet.store';

/**
 * One PU Spec Sheet for a phase (`/phases/:phaseId/sheets/:sheetTypeId`), full width. Users build it up
 * from the sheet type's table templates: adding tables, then sections and rows within them, and filling
 * in the cells. Each table, section and row is locked to its editor until they publish, and each
 * publish is a numbered version that can be viewed again by number or date. Several people can work on
 * a sheet at once: `SheetLiveStore` shows who is there and keeps their checkouts up to date.
 */
@Component({
  selector: 'app-sheet-page',
  imports: [
    Breadcrumb,
    CdkDrag,
    CdkDropList,
    ConfirmPopupModule,
    EmptyState,
    MessageModule,
    SheetLoadError,
    SheetSwitcher,
    SheetTableCard,
    SheetTakeoverRequests,
    SheetToolbar,
    SkeletonModule,
    ToastModule,
  ],
  providers: [
    ConfirmationService,
    MessageService,
    RowCheckoutStore,
    RowTakeoverStore,
    SheetHub,
    SheetLiveStore,
    SheetStore,
  ],
  templateUrl: './sheet-page.html',
  styleUrl: './sheet-page.scss',
})
export class SheetPage {
  /** Bound from the route parameters. */
  public readonly phaseId = input.required<string>();
  public readonly sheetTypeId = input.required<string>();

  public readonly phase = computed<Phase | undefined>(() =>
    this.phases.phaseById(Number(this.phaseId())),
  );

  /** The phase's available sheets, for the switcher. */
  public readonly phaseSheetTypes = computed<SheetType[]>(() => {
    const phase = this.phase();
    return phase ? this.phases.sheetTypesFor(phase) : [];
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
      ...this.phases
        .ancestorsOf(phase.id)
        .map((ancestor) => ({ label: ancestor.code, link: ['/phases', ancestor.id] })),
      { label: phase.code, link: ['/phases', phase.id] },
      { label: this.sheetType()?.name ?? '' },
    ];
  });

  public readonly sheet = computed(() => this.sheetStore.sheet());
  public readonly canEdit = computed(() => this.sheetStore.canEdit());
  public readonly error = computed(() => this.sheetStore.error());

  public readonly emptyMessage = computed(() => {
    const sheet = this.sheet();
    if (sheet === null) {
      return '';
    }
    if (!sheet.isLive) {
      return 'Nothing had been published at that point.';
    }
    return sheet.availableTemplates.length > 0
      ? "Use Add table to start from one of this sheet type's templates."
      : 'There are no table templates for this sheet type yet. Create one under Templates.';
  });

  public readonly unavailableMessage = computed(
    () => `This sheet type isn't available to ${this.phase()?.code ?? 'this phase'}.`,
  );

  public readonly isLoadingPhases = computed(() => this.phases.isLoading());

  public readonly hasPhasesError = computed(() => this.phases.hasError());

  /** Only while there is nothing to show: a refresh keeps the open sheet on screen. */
  public readonly isLoadingSheet = computed(
    () => this.sheetStore.isLoading() && this.sheet() === null,
  );

  public readonly hasSheetError = computed(
    () => this.sheetStore.hasError() && this.sheet() === null,
  );

  private readonly phases = inject(PhasesStore);
  private readonly sheetStore = inject(SheetStore);

  public retry(): void {
    this.phases.reload();
    this.sheetStore.reload();
  }

  /** A table was dragged to a new place; each dragged card carries its table's id. */
  public dropTable(event: CdkDragDrop<unknown, unknown, number>): void {
    if (event.previousIndex !== event.currentIndex) {
      void this.sheetStore.moveTableTo(event.item.data, event.currentIndex);
    }
  }

  public dismissError(): void {
    this.sheetStore.dismissError();
  }
}
