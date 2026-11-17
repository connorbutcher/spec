import { Component, inject, input } from '@angular/core';
import { SpecSheetPhase } from '../../../../core/models/domain/types/spec-sheet-phase';
import { SheetsService } from '../../services/sheets.service';
import { SheetActions } from '../sheet-actions/sheet-actions';
import { SheetTabs } from '../sheet-tabs/sheet-tabs';

@Component({
  selector: 'app-sheet-header',
  templateUrl: './sheet-header.html',
  styleUrl: './sheet-header.scss',
  imports: [SheetTabs, SheetActions],
})
export class SheetHeader {
  public readonly phase = input.required<SpecSheetPhase>();

  private readonly _sheetsService = inject(SheetsService);

  public get type() {
    return this._sheetsService.selectedType;
  }
}
