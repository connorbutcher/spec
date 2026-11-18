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
import { SheetItemLockHelpers } from '../../../core/helpers/sheet-item-lock-helpers';
import { SheetCheckoutSummary } from '../../../core/models/domain/sheets/sheet-checkout-summary';
import { SheetItemLock } from '../../../core/models/domain/sheets/sheet-item-lock';
import { SheetItemRevision } from '../../../core/models/domain/sheets/sheet-item-revision';
import { UserService } from '../../../core/services/user.service';
import { PUCardAnalyticsService } from '../../analytics/services/pu-card-analytics.service';
import { SheetsService } from '../../sheets/services/sheets.service';
import { PUSpecSheet } from '../models/pu-spec-sheet';
import { PUSpecSheetTableGroup } from '../models/pu-spec-sheet-table-group';
import { SheetContext } from './sheet-context';
import { SheetDetail } from '../../../core/models/domain/sheets/sheet-detail';

@Injectable()
export class ActiveSheetService implements SheetContext {
  public readonly groupReferenceIdsDeleted = signal<Set<string>>(
    new Set<string>(),
  );

  /**
   * Whether the sheet open is its current state, which anyone may edit: each row or table they change is checked out
   * to them. Older versions are read-only. Use canEditItem for whether a particular item can be changed.
   */
  public readonly canEdit = computed(() => {
    const specSheet = this.specSheet();
    return (
      !!specSheet &&
      !specSheet.isPublished() &&
      !this._puCardAnalyticsService.isEnabled()
    );
  });
  public readonly currentUserId = computed(() => this._userService.user()?.id);
  /** How many rows and tables are checked out, by the current user and by others. */
  public readonly checkoutSummary = computed<SheetCheckoutSummary>(() => {
    const specSheet = this.specSheet();
    if (!specSheet) {
      return { mine: 0, mineAwaitingApproval: 0, others: [] };
    }

    return SheetItemLockHelpers.summarise(
      specSheet.sheet,
      this.currentUserId(),
    );
  });
  public readonly hasChanges = computed(
    () => this.specSheet()?.hasChanges() ?? false,
  );
  public readonly externalReferences = computed(
    () => this.specSheet()?.sheet.externalReferences ?? [],
  );
  /** Whether the current user has saved checked-out changes they can submit (or publish). */
  public readonly canPublish = computed(
    () => this.canEdit() && this.checkoutSummary().mine > 0,
  );
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
  private readonly _userService = inject(UserService);
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

  /** Who has the item checked out, or undefined when anyone may edit it. */
  public lockOf(
    revision: SheetItemRevision | undefined,
  ): SheetItemLock | undefined {
    return SheetItemLockHelpers.lockOf(revision, this.currentUserId());
  }

  /** Whether the current user may change the item: the sheet is editable and nobody else has it checked out. */
  public canEditItem(revision: SheetItemRevision | undefined): boolean {
    return (
      this.canEdit() && SheetItemLockHelpers.isEditable(this.lockOf(revision))
    );
  }

  /** A group can't be removed while someone else has it, or any of its rows, checked out. */
  public canRemoveGroup(group: PUSpecSheetTableGroup): boolean {
    return (
      this.canEditItem(group.group.revision) &&
      group.rows().every((row) => this.canEditItem(row.row.revision))
    );
  }

  /** Whether the current user has saved changes to the group or its rows that they can undo. */
  public canUndoGroup(group: PUSpecSheetTableGroup): boolean {
    if (group.id <= 0) {
      return false;
    }

    return [
      group.group.revision,
      ...group.rows().map((row) => row.row.revision),
    ].some((revision) =>
      SheetItemLockHelpers.isUndoable(this.lockOf(revision)),
    );
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
