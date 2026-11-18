import {
  computed,
  inject,
  Injectable,
  Injector,
  runInInjectionContext,
  Signal,
  signal,
} from '@angular/core';
import { Subject } from 'rxjs';
import { CellLookupService } from '../../../core/services/cell-lookup.service';
import { CheckInOutService } from '../../../core/services/check-in-out.service';
import { PUCardAnalyticsService } from '../../analytics/services/pu-card-analytics.service';
import { SheetsService } from '../../sheets/services/sheets.service';
import { PUSpecSheet } from '../models/pu-spec-sheet';
import { SheetContext } from './sheet-context';
import { SheetDetail } from '../../../core/models/domain/sheets/sheet-detail';

@Injectable()
export class ActiveSheetService implements SheetContext {
  public readonly groupReferenceIdsDeleted = signal<Set<string>>(
    new Set<string>(),
  );

  public readonly canEdit = computed(() => {
    const specSheet = this.specSheet();
    if (
      !specSheet ||
      specSheet.isPublished() ||
      this._puCardAnalyticsService.isEnabled() ||
      (specSheet.requiresApprovalWorkflow &&
        specSheet.hasApprovalWorkflowStarted)
    ) {
      return false;
    }

    const checkout = this._checkInOutService
      .puSpecSheetCheckouts()
      .find((c) => c.id === specSheet.id);
    if (!checkout) {
      return false;
    }

    return (
      checkout.user.connectionId === this._checkInOutService.currentConnectionId
    );
  });
  public readonly hasChanges = computed(
    () => this.specSheet()?.hasChanges() ?? false,
  );
  public readonly externalReferences = computed(
    () => this.specSheet()?.sheet.externalReferences ?? [],
  );
  public readonly canPublish = computed(() => {
    const specSheet = this.specSheet();
    return (
      !!specSheet &&
      !specSheet.isPublished() &&
      (!specSheet.requiresApprovalWorkflow ||
        !specSheet.hasApprovalWorkflowStarted)
    );
  });
  public readonly isValid = computed(
    () => this.specSheet()?.isValid() ?? false,
  );
  public readonly tableTemplateWidgets = computed(() => {
    const specSheetType = this._sheetsService.selectedType();
    if (!specSheetType) {
      return [];
    }

    return specSheetType.tableTemplates.flatMap(
      (tt) => tt.tableTemplateWidgets,
    );
  });

  public readonly tableGroupTemplates = computed(() => {
    const specSheetType = this._sheetsService.selectedType();
    if (!specSheetType) {
      return [];
    }

    return specSheetType.tableTemplates.flatMap((tt) => tt.tableGroupTemplates);
  });
  public readonly tableGroupTemplateWidgets = computed(() => {
    const tableGroupTemplates = this.tableGroupTemplates();
    if (!tableGroupTemplates) {
      return [];
    }

    return tableGroupTemplates.flatMap((tg) => tg.tableGroupTemplateWidgets);
  });

  public readonly tableRowTemplates = computed(() => {
    const groupTemplates = this.tableGroupTemplates();
    return groupTemplates.flatMap((gt) => gt.rowTemplates);
  });
  public readonly tableCellTemplates = computed(() => {
    const rowTemplates = this.tableRowTemplates();
    return rowTemplates.flatMap((rt) => rt.cellTemplates);
  });
  public readonly tableCellTypes = computed(() => {
    const cellTemplates = this.tableCellTemplates();
    return cellTemplates
      .map((ct) => ct.tableCellType)
      .filter(
        (value, index, self) =>
          self.findIndex((tct) => tct!.id === value!.id) === index,
      );
  });

  private readonly _sheetsService = inject(SheetsService);
  private readonly _checkInOutService = inject(CheckInOutService);
  private readonly _cellLookupService = inject(CellLookupService);
  private readonly _puCardAnalyticsService = inject(PUCardAnalyticsService);
  private readonly _injector = inject(Injector);
  private readonly _reload$ = new Subject<void>();

  private readonly _specSheet = signal<PUSpecSheet>(undefined!);

  public setSpecSheet(detail: SheetDetail): void {
    this._specSheet.set(
      runInInjectionContext(
        this._injector,
        () => new PUSpecSheet(detail.sheet, detail.submissions),
      ),
    );

    this._cellLookupService.completeRegistration();
  }

  public markGroupReferenceIdAsDeleted(referenceId: string): void {
    const deletedIds = this.groupReferenceIdsDeleted();
    deletedIds.add(referenceId);
    this.groupReferenceIdsDeleted.set(deletedIds);
  }

  public clearDeletedGroupReferenceIds(): void {
    this.groupReferenceIdsDeleted.set(new Set<string>());
  }

  public isGroupReferenceIdDeleted(referenceId: string): boolean {
    return this.groupReferenceIdsDeleted().has(referenceId);
  }

  public triggerReload(): void {
    this._reload$.next();
  }

  public get specSheet(): Signal<PUSpecSheet> {
    return this._specSheet;
  }

  public get reload$(): Subject<void> {
    return this._reload$;
  }
}
