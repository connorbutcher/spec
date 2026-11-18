import { DatePipe } from '@angular/common';
import { Component, computed, inject, input, Signal } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { ButtonDirective } from 'primeng/button';
import { Divider } from 'primeng/divider';
import { Tooltip } from 'primeng/tooltip';
import { SpecSheetType } from '../../../../../../core/models/domain/types/spec-sheet-type';
import { SheetSummaryPropertiesDisplay } from '../../../../../properties/components/display/sheet-summary-properties-display/sheet-summary-properties-display';
import { SheetsService } from '../../../../../sheets/services/sheets.service';
import { SheetListItem } from '../../../../../../core/models/domain/sheets/sheet';

@Component({
  selector: 'app-spec-sheet-version-history-row',
  imports: [
    DatePipe,
    Divider,
    SheetSummaryPropertiesDisplay,
    ButtonDirective,
    Tooltip,
  ],
  templateUrl: './spec-sheet-version-history-row.html',
  styleUrl: './spec-sheet-version-history-row.scss',
})
export class SpecSheetVersionHistoryRow {
  public readonly specSheet = input.required<SheetListItem>();
  public readonly compareTo = input.required<SheetListItem | undefined>();
  public readonly safeContent = computed(() => {
    const specSheet = this.specSheet();
    return this._domSanitizer.bypassSecurityTrustHtml(
      specSheet?.version?.changeLog ?? '',
    );
  });

  private readonly _domSanitizer = inject(DomSanitizer);
  private readonly _sheetsService = inject(SheetsService);

  public compare(): void {
    const specSheet = this.specSheet();
    const compareTo = this.compareTo();
    if (!specSheet || !compareTo) {
      return;
    }

    const url = `${window.location.origin}/comparison?type=${specSheet.specSheetTypeId}&phase=${specSheet.specSheetPhaseId}&left=${compareTo.versionNumber}&right=${specSheet.versionNumber}`;
    window.open(url, '_blank');
  }

  public get specSheetType(): Signal<SpecSheetType | null> {
    return this._sheetsService.selectedType;
  }
}
