import {
  Component,
  computed,
  effect,
  inject,
  input,
  signal,
} from '@angular/core';
import { Tag } from 'primeng/tag';
import { FragmentedElementDirective } from '../../../../../core/directives/fragmented-element.directive';
import { PUSpecSheetTableBase } from '../../../../sheet/models/tables/pu-spec-sheet-table-base';
import { SpecSheetState } from '../../../models/spec-sheet-state';
import { SpecSheetStateToTagSeverityPipe } from '../../../pipes/spec-sheet-state-to-tag-severity-pipe';
import { SheetComparisonFilterService } from '../../../services/sheet-comparison-filter.service';

@Component({
  selector: 'app-sheet-table-header-comparison',
  imports: [Tag, SpecSheetStateToTagSeverityPipe, FragmentedElementDirective],
  templateUrl: './sheet-table-header-comparison.html',
  styleUrl: './sheet-table-header-comparison.scss',
})
export class SheetTableHeaderComparison {
  public readonly table = input.required<PUSpecSheetTableBase | undefined>();
  public readonly tableState = input.required<SpecSheetState>();
  public readonly isFilteredMatch = signal<boolean>(false);
  public readonly isFilteredActive = computed<boolean>(() => {
    const fragment =
      this._specSheetComparisonFilterService.activeFilterResultReferenceId();
    if (!fragment) {
      return false;
    }

    return fragment === this.table()?.referenceId();
  });

  private readonly _specSheetComparisonFilterService = inject(
    SheetComparisonFilterService,
  );

  constructor() {
    effect(() => {
      const filter = this._specSheetComparisonFilterService
        .filter()
        .toLocaleLowerCase();
      if (!filter || filter.trim().length === 0) {
        this.isFilteredMatch.set(false);
        this._specSheetComparisonFilterService.clearMatches();
        return;
      }

      const title = this.table()?.title;
      const titleString =
        title !== null && title !== undefined
          ? title.toString().toLocaleLowerCase()
          : '';

      if (titleString.includes(filter)) {
        this.isFilteredMatch.set(true);
        this._specSheetComparisonFilterService.addMatch({
          referenceId: this.table()?.referenceId() ?? '',
          tableOrder: this.table()?.displayOrder() ?? 1,
          groupOrder: 0,
          rowNumber: 0,
        });
      } else {
        this.isFilteredMatch.set(false);
        this._specSheetComparisonFilterService.removeMatch(
          this.table()?.referenceId() ?? '',
        );
      }
    });
  }
}
