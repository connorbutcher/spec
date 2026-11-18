import {
  computed,
  inject,
  Injectable,
  Injector,
  runInInjectionContext,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { combineLatest, filter, tap } from 'rxjs';
import { TableTemplate } from '../../../core/models/domain/template/table-template';
import { SpecSheetPhase } from '../../../core/models/domain/types/spec-sheet-phase';
import { SpecSheetType } from '../../../core/models/domain/types/spec-sheet-type';
import { CellLookupService } from '../../../core/services/cell-lookup.service';
import { PUSpecSheet } from '../../sheet/models/pu-spec-sheet';
import { SheetComparison } from '../models/sheet-comparison';
import { SheetDetail } from '../../../core/models/domain/sheets/sheet-detail';
import { SheetListItem } from '../../../core/models/domain/sheets/sheet-list-item';

@Injectable()
export class SheetComparisonService {
  public readonly specSheetType = signal<SpecSheetType | undefined>(undefined);
  public readonly specSheetPhase = signal<SpecSheetPhase | undefined>(
    undefined,
  );
  public readonly tableTypes = computed<TableTemplate[]>(
    () => this.specSheetType()?.tableTemplates || [],
  );
  public readonly leftPUSpecSheet = signal<PUSpecSheet | undefined>(undefined);
  public readonly rightPUSpecSheet = signal<PUSpecSheet | undefined>(undefined);
  public readonly leftSpecSheet = signal<SheetDetail | undefined>(undefined);
  public readonly rightSpecSheet = signal<SheetDetail | undefined>(undefined);
  public readonly leftVersionNumber = signal<number | undefined>(undefined);
  public readonly rightVersionNumber = signal<number | undefined>(undefined);
  public readonly specSheetVersions = signal<SheetListItem[]>([]);
  public readonly sheetComparison = signal<SheetComparison | undefined>(
    undefined,
  );

  private readonly _cellLookupService = inject(CellLookupService);
  private readonly _injector = inject(Injector);

  constructor() {
    combineLatest([
      toObservable(this.leftPUSpecSheet),
      toObservable(this.rightPUSpecSheet),
    ])
      .pipe(
        takeUntilDestroyed(),
        tap(() => {
          this._cellLookupService.reset();
          this.sheetComparison.set(undefined);
        }),
        filter(([left, right]) => !!left && !!right),
      )
      .subscribe(([left, right]) => {
        const sheetComparison = runInInjectionContext(
          this._injector,
          () => new SheetComparison(left, right),
        );
        this.sheetComparison.set(sheetComparison);
        this._cellLookupService.completeRegistration();
      });
  }

  public reset(): void {
    this.specSheetType.set(undefined);
    this.specSheetPhase.set(undefined);
    this.leftSpecSheet.set(undefined);
    this.rightSpecSheet.set(undefined);
    this.leftVersionNumber.set(undefined);
    this.rightVersionNumber.set(undefined);
    this.leftPUSpecSheet.set(undefined);
    this.rightPUSpecSheet.set(undefined);
    this.specSheetVersions.set([]);
    this.sheetComparison.set(undefined);
  }
}
