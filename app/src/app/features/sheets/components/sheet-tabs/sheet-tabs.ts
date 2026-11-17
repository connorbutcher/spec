import { Component, inject, input, Signal } from '@angular/core';
import { SpecSheetPhase } from '../../../../core/models/domain/types/spec-sheet-phase';
import { SpecSheetType } from '../../../../core/models/domain/types/spec-sheet-type';
import { SheetsService } from '../../services/sheets.service';

@Component({
  selector: 'app-sheet-tabs',
  templateUrl: './sheet-tabs.html',
  styleUrl: './sheet-tabs.scss',
})
export class SheetTabs {
  public readonly phase = input.required<SpecSheetPhase>();

  private readonly _sheetsService = inject(SheetsService);

  public setActiveType(type: SpecSheetType): void {
    this._sheetsService.selectType(type.id);
  }

  public get selectedType(): Signal<SpecSheetType | null> {
    return this._sheetsService.selectedType;
  }
}
