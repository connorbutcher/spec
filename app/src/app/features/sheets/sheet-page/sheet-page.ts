import { CdkDrag, CdkDragDrop, CdkDropList } from '@angular/cdk/drag-drop';
import { httpResource } from '@angular/common/http';
import { Component, computed, effect, inject, input } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { NavigationEnd, Router } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ConfirmPopupModule } from 'primeng/confirmpopup';
import { MessageModule } from 'primeng/message';
import { SkeletonModule } from 'primeng/skeleton';
import { ToastModule } from 'primeng/toast';
import { filter } from 'rxjs';
import { Phase } from '../../../core/models/phase.model';
import { SheetSummary } from '../../../core/models/sheet-summary.model';
import { SheetType } from '../../../core/models/sheet-type.model';
import { SheetIdentityUtil } from '../../../shared/sheet-identity.util';
import { EmptyState } from '../../../shared/components/empty-state/empty-state';
import { PhasesStore } from '../../phases/phases.store';
import { RowCheckoutStore } from '../checkouts/row-checkout.store';
import { RowTakeoverStore } from '../checkouts/row-takeover.store';
import { SheetAnchorService } from '../anchors/sheet-anchor.service';
import { SheetContextBar } from '../sheet-context-bar/sheet-context-bar';
import { SheetHub } from '../live/sheet-hub';
import { SheetLiveStore } from '../live/sheet-live.store';
import { SheetLoadError } from '../sheet-load-error/sheet-load-error';
import { SheetSearchStore } from '../search/sheet-search.store';
import { SheetPicker } from '../sheet-picker/sheet-picker';
import { SheetSwitcher } from '../sheet-switcher/sheet-switcher';
import { SheetTableCard } from '../sheet-table-card/sheet-table-card';
import { SheetTakeoverRequests } from '../sheet-takeover-requests/sheet-takeover-requests';
import { SheetToolbar } from '../sheet-toolbar/sheet-toolbar';
import { SheetStore } from '../sheet.store';

/**
 * One PU Spec Sheet for a phase (`/phases/:phaseId/sheets/:sheetTypeId`, or `.../:sheetId` for one of
 * several sheets of a type told apart by identifiers), full width. Users build it up
 * from the sheet type's table templates: adding tables, then sections and rows within them, and filling
 * in the cells. Each table, section and row is locked to its editor until they publish, and each
 * publish is a numbered version that can be viewed again by number or date. Several people can work on
 * a sheet at once: `SheetLiveStore` shows who is there and keeps their checkouts up to date.
 */
@Component({
  selector: 'app-sheet-page',
  imports: [
    CdkDrag,
    CdkDropList,
    ConfirmPopupModule,
    EmptyState,
    MessageModule,
    SheetContextBar,
    SheetLoadError,
    SheetPicker,
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
    SheetAnchorService,
    SheetHub,
    SheetLiveStore,
    SheetSearchStore,
    SheetStore,
  ],
  templateUrl: './sheet-page.html',
  styleUrl: './sheet-page.scss',
})
export class SheetPage {
  /** Bound from the route parameters. */
  public readonly phaseId = input.required<string>();
  public readonly sheetTypeId = input.required<string>();
  /** Set when the route names one of the phase's sheets of the type by id. */
  public readonly sheetId = input<string>();

  public readonly phase = computed<Phase | undefined>(() =>
    this.phases.phaseById(Number(this.phaseId())),
  );

  /** The phase's available sheets, for the switcher. */
  public readonly phaseSheetTypes = computed<SheetType[]>(() => {
    const phase = this.phase();
    return phase ? this.phases.sheetTypesFor(phase) : [];
  });

  /** The phase's sheets, of every type, for the switcher and the sheet picker. */
  public readonly phaseSheets = computed<SheetSummary[]>(() =>
    this.phaseSheetsResource.hasValue() ? this.phaseSheetsResource.value() : [],
  );

  /** The open sheet type, only if it's available to this phase. */
  public readonly sheetType = computed<SheetType | undefined>(() =>
    this.phaseSheetTypes().find((sheetType) => sheetType.id === Number(this.sheetTypeId())),
  );

  public readonly sheet = computed(() => this.sheetStore.sheet());

  /** The open sheet's id: from the route, or once loaded for a sheet opened by its type. */
  public readonly openSheetId = computed<number | null>(() => {
    const fromRoute = Number(this.sheetId());
    return Number.isInteger(fromRoute) && fromRoute > 0 ? fromRoute : (this.sheet()?.id ?? null);
  });

  /** The open sheet's identifier values, e.g. "ESN-100 · Build A"; empty for a type without identifiers. */
  public readonly identityLabel = computed(() => {
    const sheetType = this.sheetType();
    const summary = this.phaseSheets().find((candidate) => candidate.id === this.openSheetId());
    return sheetType && sheetType.identifiers.length > 0 && summary ? SheetIdentityUtil.label(summary, sheetType) : '';
  });
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
      ? "Use Add to start a table from one of this sheet type's templates."
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
  private readonly title = inject(Title);

  private readonly phaseSheetsResource = httpResource<SheetSummary[]>(() => `/api/phases/${this.phaseId()}/sheets`);

  private readonly navigationEnd = toSignal(
    inject(Router).events.pipe(filter((event) => event instanceof NavigationEnd)),
  );

  /**
   * Names the browser tab after the phase and sheet, so several open sheets can be told apart. It is set
   * again after every navigation, because moving between rows changes the URL's fragment and the router
   * then puts the route's generic title back.
   */
  constructor() {
    effect(() => {
      this.navigationEnd();
      const phase = this.phase();
      const sheetType = this.sheetType();
      const identity = this.identityLabel();
      const name = phase && sheetType ? `${phase.code} ${sheetType.name}${identity ? ` ${identity}` : ''}` : 'Sheet';
      this.title.setTitle(`${name} · PU Spec Sheet`);
    });
  }

  public retry(): void {
    this.phases.reload();
    this.phaseSheetsResource.reload();
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
