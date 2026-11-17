import { inject, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { combineLatest, distinctUntilChanged, filter, map } from 'rxjs';
import { PUSpecSheetTableCellBase } from '../../features/sheet/models/cells/pu-spec-sheet-table-cell-base';
import { PUSpecSheetTableGroup } from '../../features/sheet/models/pu-spec-sheet-table-group';
import { PUSpecSheetTableRow } from '../../features/sheet/models/pu-spec-sheet-table-row';
import { PUSpecSheetTableBase } from '../../features/sheet/models/tables/pu-spec-sheet-table-base';
import { ActivationService } from './activation.service';
import { CellLookupService } from './cell-lookup.service';

export type ActiveFragment = {
  fragment: string;
  focus: boolean;
};

@Injectable()
export class FragmentHandler {
  public readonly activeFragment = signal<ActiveFragment | undefined>(
    undefined,
  );

  private readonly _activatedRoute = inject(ActivatedRoute);
  private readonly _cellLookupService = inject<
    CellLookupService<
      PUSpecSheetTableCellBase,
      PUSpecSheetTableBase,
      PUSpecSheetTableGroup,
      PUSpecSheetTableRow
    >
  >(
    CellLookupService<
      PUSpecSheetTableCellBase,
      PUSpecSheetTableBase,
      PUSpecSheetTableGroup,
      PUSpecSheetTableRow
    >,
  );
  private readonly _activationService = inject(ActivationService);

  constructor() {
    const ready$ = toObservable(
      this._cellLookupService.registrationComplete,
    ).pipe(distinctUntilChanged());

    combineLatest([
      this._activatedRoute.queryParamMap.pipe(
        map((params) => params.get('focusFragment')),
      ),
      this._activatedRoute.fragment,
      ready$,
    ])
      .pipe(
        takeUntilDestroyed(),
        filter(([, , ready]) => ready),
        map(([focusCell, fragment, _]) => [focusCell, fragment]),
        distinctUntilChanged((a, b) => a[0] === b[0] && a[1] === b[1]),
      )
      .subscribe(([focusFragment, fragment]) => {
        if (!fragment) {
          return;
        }

        const focus = focusFragment === 'true';

        const cell = this._cellLookupService.getCell(fragment);
        if (cell) {
          this.activeFragment.set({ fragment, focus });

          if (focus) {
            this._activationService.refToActivate.set(cell.referenceId());
          }
          return;
        }

        const table = this._cellLookupService.getTable(fragment);
        if (table) {
          this.activeFragment.set({ fragment, focus });

          if (focus) {
            this._activationService.refToActivate.set(table.referenceId());
          }
        }

        const group = this._cellLookupService.getGroup(fragment);
        if (group) {
          this.activeFragment.set({ fragment, focus });

          if (focus) {
            this._activationService.refToActivate.set(group.referenceId());
          }
        }

        const row = this._cellLookupService.getRow(fragment);
        if (row) {
          this.activeFragment.set({ fragment, focus });

          if (focus) {
            this._activationService.refToActivate.set(row.referenceId());
          }
        }
      });
  }
}
