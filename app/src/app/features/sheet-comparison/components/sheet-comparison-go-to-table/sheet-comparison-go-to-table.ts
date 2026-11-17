import { Component, computed, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonDirective } from 'primeng/button';
import { Popover } from 'primeng/popover';
import { TableComparison } from '../../models/table-comparison';
import { SheetComparisonService } from '../../services/sheet-comparison.service';

@Component({
  selector: 'app-sheet-comparison-go-to-table',
  imports: [ButtonDirective, Popover],
  templateUrl: './sheet-comparison-go-to-table.html',
  styleUrl: './sheet-comparison-go-to-table.scss',
})
export class SheetComparisonGoToTable {
  public readonly comparisonTables = computed(
    () =>
      this._sheetComparisonService.sheetComparison()?.tableComparisons || [],
  );

  private readonly _sheetComparisonService = inject(SheetComparisonService);
  private readonly _activatedRoute = inject(ActivatedRoute);
  private readonly _router = inject(Router);

  public goToTable(table: TableComparison): void {
    this._router.navigate([], {
      relativeTo: this._activatedRoute,
      fragment: table.referenceId,
      queryParamsHandling: 'merge',
    });
  }
}
