import {
  computed,
  inject,
  Injectable,
  Injector,
  runInInjectionContext,
  signal,
} from '@angular/core';
import { CellLookupService } from '../../../core/services/cell-lookup.service';
import { PUSpecSheet } from '../../sheet/models/pu-spec-sheet';
import { SheetContext } from '../../sheet/services/sheet-context';
import { SheetComparisonService } from './sheet-comparison.service';
import { SheetDetail } from '../../../core/models/domain/sheets/sheet';

@Injectable()
export class SheetComparisonContextService implements SheetContext {
  public readonly specSheet = signal<PUSpecSheet>(undefined!);
  public readonly externalReferences = computed(
    () => this.specSheet()?.sheet.externalReferences ?? [],
  );
  public readonly tableTemplateWidgets = computed(() =>
    this._comparisonService
      .tableTypes()
      .flatMap((table) => table.tableTemplateWidgets),
  );
  public readonly tableGroupTemplates = computed(() =>
    this._comparisonService
      .tableTypes()
      .flatMap((table) => table.tableGroupTemplates),
  );
  public readonly tableGroupTemplateWidgets = computed(() =>
    this.tableGroupTemplates().flatMap(
      (group) => group.tableGroupTemplateWidgets,
    ),
  );
  public readonly tableRowTemplates = computed(() =>
    this.tableGroupTemplates().flatMap((group) => group.rowTemplates),
  );

  private readonly _comparisonService = inject(SheetComparisonService);
  private readonly _cellLookupService = inject(CellLookupService);
  private readonly _injector = inject(Injector);
  private readonly _deletedGroupReferenceIds = new Set<string>();

  public setSpecSheet(detail: SheetDetail): void {
    const type = this._comparisonService.specSheetType();
    const phase = this._comparisonService.specSheetPhase();
    if (!type || !phase) {
      throw new Error(
        'Comparison type and phase must be selected before loading a sheet.',
      );
    }

    this._deletedGroupReferenceIds.clear();
    this.specSheet.set(
      runInInjectionContext(
        this._injector,
        () =>
          new PUSpecSheet(
            {
              ...detail.sheet,
              specSheetType: type,
              specSheetPhase: phase,
            },
            detail.submissions,
          ),
      ),
    );
    this._cellLookupService.completeRegistration();
  }

  public markGroupReferenceIdAsDeleted(referenceId: string): void {
    this._deletedGroupReferenceIds.add(referenceId);
  }

  public isGroupReferenceIdDeleted(referenceId: string): boolean {
    return this._deletedGroupReferenceIds.has(referenceId);
  }
}
