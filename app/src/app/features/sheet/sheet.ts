import { Dialog } from '@angular/cdk/dialog';
import { SheetPropertyHelpers } from '../../core/helpers/sheet-property-helpers';
import {
  AfterViewInit,
  Component,
  computed,
  DestroyRef,
  effect,
  Injector,
  input,
  resource,
  runInInjectionContext,
  Signal,
  viewChild,
  ViewContainerRef,
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { Title } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { ButtonDirective } from 'primeng/button';
import {
  combineLatest,
  distinctUntilChanged,
  filter,
  firstValueFrom,
  switchMap,
} from 'rxjs';
import { CheckoutRequest } from '../../core/models/check-in-out/checkout-request';
import { CellLookupService } from '../../core/services/cell-lookup.service';
import { CheckInOutService } from '../../core/services/check-in-out.service';
import { FragmentHandler } from '../../core/services/fragment-handler.service';
import { PhasesService } from '../../core/services/phases.service';
import { StructureLevelType } from '../../core/types/configuration/structure-level-type.enum';
import { DeactivationType } from '../../core/types/deactivation-type.enum';
import { AnalyticsDataPointConfiguration } from '../analytics/components/analytics-data-point-configuration/analytics-data-point-configuration';
import { PUCardAnalyticsService } from '../analytics/services/pu-card-analytics.service';
import { CanDeactivate } from '../common/can-deactivate/can-deactivate';
import { CheckoutRequestedDialog } from '../common/checkout-requested-dialog/checkout-requested-dialog';
import { EmptyState } from '../common/empty-state/empty-state';
import { ErroredBlock } from '../common/errored-block/errored-block';
import { LoadingBlock } from '../common/loading-block/loading-block';
import { SheetsService } from '../sheets/services/sheets.service';
import { SidePanel } from '../side-panel/side-panel';
import { SidePanelConfigService } from '../side-panel/side-panel-config.service';
import { SidePanelService } from '../side-panel/side-panel.service';
import { SpecSheetToolbar } from './components/spec-sheet-toolbar/spec-sheet-toolbar';
import { CreatePuSpecSheetDialog } from './dialogs/create-pu-spec-sheet-dialog/create-pu-spec-sheet-dialog';
import { CreatePUSpecSheetDialogData } from './dialogs/create-pu-spec-sheet-dialog/models/create-pu-spec-sheet-dialog-data';
import { HorizontalLayout } from './features/layouts/horizontal-layout/horizontal-layout';
import { VerticalLayout } from './features/layouts/vertical-layout/vertical-layout';
import { PUSpecSheetTableCellBase } from './models/cells/pu-spec-sheet-table-cell-base';
import { PUSpecSheetTableGroup } from './models/pu-spec-sheet-table-group';
import { PUSpecSheetTableRow } from './models/pu-spec-sheet-table-row';
import { PUSpecSheetTableBase } from './models/tables/pu-spec-sheet-table-base';
import { ActiveSheetService } from './services/active-sheet.service';
import { LinkedSheetService } from './services/linked-sheet.service';
import { RowContextMenuService } from './services/row-context-menu.service';
import { SelectionService } from './services/selection.service';
import { SheetContext } from './services/sheet-context';
import { SheetDataService } from './services/sheet-data.service';
import { SheetFilterService } from './services/sheet-filter.service';
import { TableGroupBuilderService } from './services/table-group-builder.service';

@Component({
  selector: 'app-sheet',
  imports: [
    LoadingBlock,
    ErroredBlock,
    SidePanel,
    SpecSheetToolbar,
    EmptyState,
    ButtonDirective,
  ],
  templateUrl: './sheet.html',
  styleUrl: './sheet.scss',
  providers: [
    ActiveSheetService,
    { provide: SheetContext, useExisting: ActiveSheetService },
    SheetDataService,
    SheetFilterService,
    FragmentHandler,
    CellLookupService,
    RowContextMenuService,
    LinkedSheetService,
    TableGroupBuilderService,
    SidePanelConfigService,
    SelectionService,
  ],
})
export class Sheet extends CanDeactivate implements AfterViewInit {
  public readonly id = input<number>();
  public readonly toolbarComponent = viewChild(SpecSheetToolbar);
  public readonly specSheetContent = viewChild('specSheetContent', {
    read: ViewContainerRef,
  });

  public readonly specSheetResource = resource({
    params: () => {
      const id = this.id();
      if (id) {
        return { sheetId: id, versionNumber: undefined, isDraft: false };
      }

      const selected = this._sheetsService?.selectedSpecSheet();
      return selected
        ? {
            sheetId: selected.sheetId,
            versionNumber: selected.versionNumber,
            isDraft: selected.isDraft,
          }
        : undefined;
    },
    loader: ({ params }) =>
      params.isDraft
        ? this._sheetDataService.getDraftSheetAsync(params.sheetId)
        : this._sheetDataService.getPublishedSheetAsync(
            params.sheetId,
            params.versionNumber,
          ),
  });

  public readonly isLoading = computed(() => {
    if (
      (!this.id() && this._sheetsService.isLoading()) ||
      this.specSheetResource.isLoading()
    ) {
      return true;
    }
    if (this.specSheetResource.error()) {
      return false;
    }

    if (!this.specSheetResource.value()) {
      return false;
    }

    const activeSheet = this._sheetService.specSheet();
    return (
      !activeSheet || activeSheet.sheet !== this.specSheetResource.value()!.sheet
    );
  });
  public readonly error = computed<string[]>(() => {
    const error = this.specSheetResource.error();
    const errorMessages: string[] = [];
    if (error) {
      errorMessages.push('Failed to load spec sheet.');
    }

    return errorMessages;
  });

  private readonly _specSheet = computed(() => this.specSheetResource.value());

  constructor(
    private readonly _sheetService: ActiveSheetService,
    private readonly _sheetsService: SheetsService,
    private readonly _phasesService: PhasesService,
    private readonly _sheetDataService: SheetDataService,
    private readonly _cellLookupService: CellLookupService<
      PUSpecSheetTableCellBase,
      PUSpecSheetTableBase,
      PUSpecSheetTableGroup,
      PUSpecSheetTableRow
    >,
    private readonly _linkedSheetService: LinkedSheetService,
    private readonly _fragmentHandler: FragmentHandler,
    private readonly _sidePanelService: SidePanelService,
    private readonly _checkInOutService: CheckInOutService,
    private readonly _puCardAnalyticsService: PUCardAnalyticsService,
    private readonly _sidePanelConfigService: SidePanelConfigService,
    private readonly _titleService: Title,
    private readonly _injector: Injector,
    private readonly _destroyRef: DestroyRef,
    private readonly _dialog: Dialog,
    private readonly _router: Router,
  ) {
    super();

    this._sidePanelConfigService.panelWidth.set(350);
    this._sidePanelConfigService.canNavigate.set(false);
    this._sidePanelConfigService.canClose.set(false);

    this._sheetService.reload$.pipe(takeUntilDestroyed()).subscribe(() => {
      this.specSheetResource.reload();
    });

    combineLatest([
      toObservable(this._specSheet),
      toObservable(this._sheetsService.selectedType),
    ])
      .pipe(
        takeUntilDestroyed(),
        filter(([s, st]) => !!s && !!st),
      )
      .subscribe(([detail, specSheetType]) => {
        if (!detail || !specSheetType) {
          return;
        }

        const specSheet = detail.sheet;
        runInInjectionContext(this._injector, () => {
          this._cellLookupService.reset();
          this._linkedSheetService.reset();
          this._sheetService.setSpecSheet(detail);
        });

        const displayStructureLevel = specSheetType.structureLevels.find(
          (sl) => sl.structureLevelType === StructureLevelType.Display,
        );
        if (!displayStructureLevel) {
          return;
        }

        let title = `${specSheetType.type} - ${this._phasesService.selectedPhase()?.phase ?? ''}`;
        for (const levelValue of displayStructureLevel.levelValues) {
          const property = SheetPropertyHelpers.findByReferenceId(
            specSheet.properties,
            specSheetType.properties,
            levelValue.specSheetTypePropertyReferenceId,
          );
          if (property?.value) {
            title += ` - ${SheetPropertyHelpers.getText(property)}`;
          }
        }

        this._titleService.setTitle(title);
      });

    effect(() => {
      const tableContent = this.specSheetContent();
      const specSheet = this._specSheet();
      if (!tableContent || !specSheet) {
        return;
      }

      this.setContent();
    });

    combineLatest([
      toObservable(this._checkInOutService.isServiceConnected),
      toObservable(this._sheetService.specSheet),
    ])
      .pipe(
        takeUntilDestroyed(),
        distinctUntilChanged(),
        filter(
          ([isConnected, specSheet]) =>
            isConnected &&
            !!specSheet &&
            !specSheet.isPublished() &&
            (!specSheet.requiresApprovalWorkflow ||
              !specSheet.hasApprovalWorkflowStarted) &&
            !this._checkInOutService.isPUSpecSheetCheckedOut(specSheet.id),
        ),
        switchMap(([_, specSheet]) =>
          this._checkInOutService.invokeCheckoutAsync(specSheet.id),
        ),
      )
      .subscribe();

    this._checkInOutService.checkoutRequested$
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe(async (checkoutRequest) => {
        if (!checkoutRequest) {
          return;
        }

        const dialogRef = this._dialog.open<
          boolean | undefined,
          CheckoutRequest,
          CheckoutRequestedDialog
        >(CheckoutRequestedDialog, {
          data: checkoutRequest,
        });

        const result = await firstValueFrom(dialogRef.closed);
        if (result === undefined) {
          return;
        }

        await this._checkInOutService.invokeRespondToCheckoutRequestAsync(
          checkoutRequest.requestContextId,
          result,
        );
      });

    this.handleAnalyticsConfiguration();
  }

  ngAfterViewInit(): void {
    this.setContent();
  }

  public handleAnalyticsConfiguration(): void {
    if (!this._puCardAnalyticsService.isEnabled()) {
      return;
    }

    this._sidePanelService.push(
      AnalyticsDataPointConfiguration,
      {},
      'analytics-data-point-configuration',
    );
  }

  public async createSpecSheetAsync(): Promise<void> {
    const dialogRef = this._dialog.open<
      number | undefined,
      CreatePUSpecSheetDialogData,
      CreatePuSpecSheetDialog
    >(CreatePuSpecSheetDialog, {
      injector: this._injector,
      data: {
        type: this._sheetsService.selectedType()!,
        phase: this._phasesService.selectedPhase()!,
      },
    });

    const result = await firstValueFrom(dialogRef.closed);
    if (!result) {
      return;
    }

    await this._router.navigate([], {
      queryParams: {},
      queryParamsHandling: 'replace',
    });
    this._sheetsService.reload();
    this._sheetService.triggerReload();
  }

  public override hasChanges(): boolean {
    return (
      this._sheetService.canEdit() &&
      this._sheetService.hasChanges() &&
      this._sheetService.isValid()
    );
  }

  public override async handleExitAsync(
    deactivationType: DeactivationType,
  ): Promise<void> {
    const specSheet = this._sheetService.specSheet();
    const specSheetType = this._sheetsService.selectedType();
    if (!specSheet || !specSheetType) {
      return;
    }

    if (!this._checkInOutService.isPUSpecSheetCheckedOutToYou(specSheet.id)) {
      return;
    }

    if (deactivationType === DeactivationType.SaveAndExit) {
      await this.toolbarComponent()?.saveChangesAsync();
    }

    await this._checkInOutService.invokeCheckInAsync(specSheet.id);
  }

  public get showSidePanel(): Signal<boolean> {
    return this._sidePanelService.showPanel;
  }

  private setContent(): void {
    const tableContent = this.specSheetContent();
    if (!tableContent) {
      return;
    }

    const specSheet = this._specSheet();
    if (!specSheet) {
      return;
    }

    const specSheetType = this._sheetsService.selectedType();
    if (!specSheetType) {
      return;
    }

    tableContent.clear();

    const component = (
      specSheetType.tableOrientation === 'vertical'
        ? VerticalLayout
        : HorizontalLayout
    ) as any;
    if (!component) {
      return;
    }

    tableContent.createComponent(component);
  }
}
