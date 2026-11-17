import { A11yModule } from '@angular/cdk/a11y';
import {
  Component,
  computed,
  effect,
  inject,
  input,
  Signal,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FocusDirective } from '../../../../../core/directives/focus.directive';
import { PUSpecSheetTableBase } from '../../../models/tables/pu-spec-sheet-table-base';
import { ActiveSheetService } from '../../../services/active-sheet.service';
import { SheetFilterService } from '../../../services/sheet-filter.service';

@Component({
  selector: 'app-table-title-input',
  imports: [FormsModule, A11yModule, FocusDirective],
  templateUrl: './table-title-input.html',
  styleUrl: './table-title-input.scss',
})
export class TableTitleInput {
  public readonly table = input.required<PUSpecSheetTableBase>();
  public readonly isFilteredMatch = signal<boolean>(false);
  public readonly isFilteredActive = computed<boolean>(() => {
    const fragment = this._sheetFilterService.activeFilterResultReferenceId();
    if (!fragment) {
      return false;
    }

    return fragment === this.table().referenceId();
  });
  public readonly isFocused = signal<boolean>(false);

  private readonly _sheetFilterService = inject(SheetFilterService);
  private readonly _activeSheetService = inject(ActiveSheetService);

  constructor() {
    effect(() => {
      const filter = this._sheetFilterService.filter().toLocaleLowerCase();
      if (!filter || filter.trim().length === 0) {
        this.isFilteredMatch.set(false);
        this._sheetFilterService.clearMatches();
        return;
      }

      const title = this.table().title();
      const titleString =
        title !== null && title !== undefined
          ? title.toString().toLocaleLowerCase()
          : '';

      if (titleString.includes(filter)) {
        this.isFilteredMatch.set(true);
        this._sheetFilterService.addMatch({
          referenceId: this.table().referenceId(),
          tableOrder: this.table().displayOrder(),
          groupOrder: 0,
          rowNumber: 0,
        });
      } else {
        this.isFilteredMatch.set(false);
        this._sheetFilterService.removeMatch(this.table().referenceId());
      }
    });
  }

  public focus(event: any): void {
    if (!this.canEdit()) {
      return;
    }

    if (!event) {
      this.isFocused.set(false);
      return;
    }

    this.isFocused.set(true);
  }

  public get canEdit(): Signal<boolean> {
    return this._activeSheetService.canEdit;
  }
}
