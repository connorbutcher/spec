import {
  Component,
  effect,
  input,
  numberAttribute,
  Signal,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import {
  combineLatest,
  distinctUntilChanged,
  filter,
  map,
  switchMap,
  tap,
} from 'rxjs';
import { SpecSheetHelpers } from '../../core/helpers/spec-sheet-helpers';
import { SpecSheetPhaseHelpers } from '../../core/helpers/spec-sheet-phase-helpers';
import { CellLookupService } from '../../core/services/cell-lookup.service';
import { FragmentHandler } from '../../core/services/fragment-handler.service';
import { PhasesService } from '../../core/services/phases.service';
import { TypesService } from '../../core/services/types.service';
import { SheetDataService } from '../sheet/services/sheet-data.service';
import { SheetFilterService } from '../sheet/services/sheet-filter.service';
import { SheetCompare } from './components/sheet-compare/sheet-compare';
import { SheetComparisonHeader } from './components/sheet-comparison-header/sheet-comparison-header';
import { SheetComparisonDataService } from './services/sheet-comparison-data.service';
import { SheetComparisonFilterService } from './services/sheet-comparison-filter.service';
import { SheetComparisonService } from './services/sheet-comparison.service';

@Component({
  selector: 'app-sheet-comparison',
  imports: [SheetComparisonHeader, SheetCompare],
  templateUrl: './sheet-comparison.html',
  styleUrl: './sheet-comparison.scss',
  providers: [
    SheetComparisonService,
    SheetComparisonDataService,
    SheetDataService,
    SheetComparisonFilterService,
    FragmentHandler,
    CellLookupService,
    {
      provide: SheetFilterService,
      useExisting: SheetComparisonFilterService,
    },
  ],
})
export class SheetComparison {
  public readonly type = input.required({ transform: numberAttribute });
  public readonly phase = input.required({ transform: numberAttribute });
  public readonly left = input.required({
    transform: numberAttribute,
  });
  public readonly right = input.required({
    transform: numberAttribute,
  });

  public readonly hasSeeded = signal(false);

  constructor(
    private readonly _specSheetComparisonService: SheetComparisonService,
    private readonly _specSheetComparisonDataService: SheetComparisonDataService,
    private readonly _phasesService: PhasesService,
    private readonly _typesService: TypesService,
    private readonly _fragmentHandler: FragmentHandler,
  ) {
    effect(() => {
      const typeId = this.type();
      const phaseId = this.phase();
      const types = this._typesService.specSheetTypes();
      const phases = this._phasesService.specSheetPhases();
      if (types.length === 0 || phases.length === 0) {
        return;
      }

      this._specSheetComparisonService.reset();

      if (typeId) {
        const type = types.find((t) => t.id === typeId);
        this._specSheetComparisonService.specSheetType.set(type);
      }

      if (phaseId) {
        const phase = SpecSheetPhaseHelpers.getPhaseByIdFromArray(
          phases,
          phaseId,
        );
        this._specSheetComparisonService.specSheetPhase.set(phase);
      }

      this.hasSeeded.set(true);
    });

    this.loadSide(this.left, 'left');
    this.loadSide(this.right, 'right');

    combineLatest([
      toObservable(this.type),
      toObservable(this.phase),
      toObservable(this._specSheetComparisonService.specSheetType),
    ])
      .pipe(
        takeUntilDestroyed(),
        filter(
          ([typeId, phaseId, specSheetType]) =>
            !!typeId && !!phaseId && !!specSheetType,
        ),
        switchMap(([typeId, phaseId]) =>
          this._specSheetComparisonDataService.getAllSpecSheetsByPhaseAndTypeAsync(
            phaseId,
            typeId,
          ),
        ),
      )
      .subscribe((specSheets) => {
        this._specSheetComparisonService.specSheetVersions.set(
          SpecSheetHelpers.sortSummaries(
            specSheets,
            this._specSheetComparisonService.specSheetType()!,
          ),
        );
      });
  }

  /** Loads the version of the sheet (identified by its version number) for a side of the comparison. */
  private loadSide(
    versionNumber: Signal<number>,
    side: 'left' | 'right',
  ): void {
    const comparison = this._specSheetComparisonService;
    const sheet =
      side === 'left' ? comparison.leftSpecSheet : comparison.rightSpecSheet;
    const version =
      side === 'left'
        ? comparison.leftVersionNumber
        : comparison.rightVersionNumber;

    combineLatest([
      toObservable(versionNumber),
      toObservable(comparison.specSheetVersions),
    ])
      .pipe(
        takeUntilDestroyed(),
        distinctUntilChanged(
          ([previousNumber, previousVersions], [number, versions]) =>
            previousNumber === number &&
            previousVersions.find((v) => v.versionNumber === number) ===
              versions.find((v) => v.versionNumber === number),
        ),
        tap(([number]) => {
          version.set(number || undefined);
          sheet.set(undefined);
        }),
        map(([number, versions]) =>
          versions.find((v) => v.versionNumber === number),
        ),
        filter((item) => !!item),
        switchMap((item) =>
          this._specSheetComparisonDataService.getSheetAsync(item!),
        ),
      )
      .subscribe((detail) => sheet.set(detail));
  }
}
