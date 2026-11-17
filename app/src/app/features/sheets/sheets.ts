import { Component, effect, inject } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { RouterOutlet } from '@angular/router';
import { PhasesService } from '../../core/services/phases.service';
import { EmptyState } from '../common/empty-state/empty-state';
import { SheetHeader } from './components/sheet-header/sheet-header';
import { SheetsDataService } from './services/sheets-data.service';
import { SheetsService } from './services/sheets.service';

@Component({
  selector: 'app-sheets',
  imports: [SheetHeader, RouterOutlet, EmptyState],
  templateUrl: './sheets.html',
  styleUrl: './sheets.scss',
  providers: [SheetsService, SheetsDataService],
})
export class Sheets {
  private readonly _phasesService = inject(PhasesService);
  private readonly _sheetsService = inject(SheetsService);
  private readonly _title = inject(Title);

  constructor() {
    effect(() => {
      const selectedPhase = this._phasesService.selectedPhase();
      const selectedTypeId = this._sheetsService.selectedTypeId();
      const firstType = selectedPhase?.types[0];

      if (selectedTypeId === null && firstType) {
        this._sheetsService.selectType(firstType.id);
      }
    });

    effect(() => {
      const selectedPhase = this._phasesService.selectedPhase();
      const selectedType = this._sheetsService.selectedType();

      let title = '';
      if (!selectedPhase && !selectedType) {
        this._title.setTitle('PU Spec Sheet');
        return;
      }

      if (selectedPhase) {
        title += selectedPhase.phase;
      }

      if (selectedType) {
        title += title ? ' - ' : '';
        title += selectedType.type;
      }

      this._title.setTitle(title);
    });
  }

  public get selectedPhase() {
    return this._phasesService.selectedPhase;
  }

  public get selectedType() {
    return this._sheetsService.selectedType;
  }
}
