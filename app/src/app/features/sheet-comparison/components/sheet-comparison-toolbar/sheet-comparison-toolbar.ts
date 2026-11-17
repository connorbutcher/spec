import { Component, inject, Signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Select } from 'primeng/select';
import { SpecSheetSearch } from '../../../sheet/components/spec-sheet-search/spec-sheet-search';
import { SpecSheetState, states } from '../../models/spec-sheet-state';
import { SpecSheetStateToBackgroundColorPipe } from '../../pipes/spec-sheet-state-to-background-color-pipe';
import { SpecSheetStateToDisplayNamePipe } from '../../pipes/spec-sheet-state-to-display-name-pipe';
import { SheetComparisonFilterService } from '../../services/sheet-comparison-filter.service';
import { SheetComparisonGoToTable } from '../sheet-comparison-go-to-table/sheet-comparison-go-to-table';

@Component({
  selector: 'app-sheet-comparison-toolbar',
  imports: [
    FormsModule,
    Select,
    SpecSheetSearch,
    SpecSheetStateToBackgroundColorPipe,
    SpecSheetStateToDisplayNamePipe,
    SheetComparisonGoToTable,
  ],
  templateUrl: './sheet-comparison-toolbar.html',
  styleUrl: './sheet-comparison-toolbar.scss',
})
export class SpecSheetComparisonToolbar {
  public readonly stateOptions = [...states];

  private readonly _specSheetComparisonFilterService = inject(
    SheetComparisonFilterService,
  );

  public get specSheetStates(): Signal<SpecSheetState[]> {
    return this._specSheetComparisonFilterService.selectedStates;
  }
}
