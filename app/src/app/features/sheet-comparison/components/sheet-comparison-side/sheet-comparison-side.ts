import {
  AfterViewInit,
  Component,
  computed,
  effect,
  inject,
  Injector,
  input,
  runInInjectionContext,
  viewChild,
  ViewContainerRef,
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { filter } from 'rxjs/operators';
import { UpdateHelpers } from '../../../../core/helpers/update-helpers';
import { LinkedSheetService } from '../../../sheet/services/linked-sheet.service';
import { SheetContext } from '../../../sheet/services/sheet-context';
import { states } from '../../models/spec-sheet-state';
import { SheetComparisonContextService } from '../../services/sheet-comparison-context.service';
import { SheetComparisonFilterService } from '../../services/sheet-comparison-filter.service';
import { SheetComparisonService } from '../../services/sheet-comparison.service';
import { HorizontalComparisonBlock } from '../horizontal/horizontal-comparison-block/horizontal-comparison-block';
import { VerticalComparisonBlock } from '../vertical/vertical-comparison-block/vertical-comparison-block';

@Component({
  selector: 'app-sheet-comparison-side',
  templateUrl: './sheet-comparison-side.html',
  styleUrl: './sheet-comparison-side.scss',
  providers: [
    SheetComparisonContextService,
    { provide: SheetContext, useExisting: SheetComparisonContextService },
    LinkedSheetService,
  ],
})
export class SheetComparisonSide implements AfterViewInit {
  public readonly side = input.required<'left' | 'right'>();
  public readonly sheet = computed(() => {
    const side = this.side();
    return side === 'left'
      ? this._sheetComparisonService.leftSpecSheet()
      : this._sheetComparisonService.rightSpecSheet();
  });

  public readonly comparisonTablesViewRef = viewChild.required(
    'comparisonTables',
    {
      read: ViewContainerRef,
    },
  );

  private readonly _comparisonTables = computed(() => {
    const selectedStates = this._sheetComparisonFilterService.selectedStates();
    const sheetComparison = this._sheetComparisonService.sheetComparison();
    if (!sheetComparison) {
      return [];
    }

    if (UpdateHelpers.isEqual(selectedStates, [...states])) {
      return sheetComparison.tableComparisons;
    }

    return sheetComparison.tableComparisons.filter((table) =>
      table.states.some((state) => selectedStates.includes(state)),
    );
  });

  private readonly _sheetComparisonService = inject(SheetComparisonService);
  private readonly _sheetComparisonFilterService = inject(
    SheetComparisonFilterService,
  );
  private readonly _sheetContext = inject(SheetComparisonContextService);
  private readonly _injector = inject(Injector);

  constructor() {
    effect(() => {
      const comparisonTables = this._comparisonTables();
      const comparisonTablesViewRef = this.comparisonTablesViewRef();
      if (!comparisonTablesViewRef || !comparisonTables) {
        return;
      }

      this.buildComparisonTables();
    });

    toObservable(
      computed(() => ({
        sheet: this.sheet(),
        type: this._sheetComparisonService.specSheetType(),
        phase: this._sheetComparisonService.specSheetPhase(),
      })),
    )
      .pipe(
        takeUntilDestroyed(),
        filter(({ sheet, type, phase }) => !!sheet && !!type && !!phase),
      )
      .subscribe(({ sheet }) => {
        if (!sheet) {
          return;
        }

        runInInjectionContext(this._injector, () => {
          this._sheetContext.setSpecSheet(sheet);
          this.side() === 'left'
            ? this._sheetComparisonService.leftPUSpecSheet.set(
                this._sheetContext.specSheet(),
              )
            : this._sheetComparisonService.rightPUSpecSheet.set(
                this._sheetContext.specSheet(),
              );
        });
      });
  }

  ngAfterViewInit(): void {
    this.buildComparisonTables();
  }

  private buildComparisonTables(): void {
    const viewRef = this.comparisonTablesViewRef();
    if (!viewRef) {
      return;
    }

    viewRef.clear();

    for (let i = 0; i < this._comparisonTables().length; i++) {
      const comparisonTable = this._comparisonTables()[i];
      const componentRef = viewRef.createComponent(
        this._sheetComparisonService.specSheetType()?.tableOrientation ===
          'horizontal'
          ? HorizontalComparisonBlock
          : VerticalComparisonBlock,
      );
      componentRef.setInput('comparisonTable', comparisonTable);
      componentRef.setInput('side', this.side());
      componentRef.setInput('gridRow', i + 1);
    }
  }
}
