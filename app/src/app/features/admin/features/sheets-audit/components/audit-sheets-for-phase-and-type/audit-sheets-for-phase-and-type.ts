import { Component, computed, inject, input, signal } from '@angular/core';
import { SpecSheetHelpers } from '../../../../../../core/helpers/spec-sheet-helpers';
import { SpecSheetPhase } from '../../../../../../core/models/domain/types/spec-sheet-phase';
import { SpecSheetType } from '../../../../../../core/models/domain/types/spec-sheet-type';
import { SheetsAuditService } from '../../sheets-audit.service';
import { SheetForPhaseRow } from '../sheet-for-phase-row/sheet-for-phase-row';

@Component({
  selector: 'app-audit-sheets-for-phase-and-type',
  imports: [SheetForPhaseRow],
  templateUrl: './audit-sheets-for-phase-and-type.html',
  styleUrl: './audit-sheets-for-phase-and-type.scss',
})
export class AuditSheetsForPhaseAndType {
  public readonly activePhase = input.required<SpecSheetPhase>();
  public readonly activeType = input.required<SpecSheetType>();
  public readonly isSaving = signal(false);
  public readonly specSheets = computed(() =>
    SpecSheetHelpers.sortSummaries(
      this._auditService.specSheetsResource.value() ?? [],
      this.activeType(),
    ),
  );

  private readonly _auditService = inject(SheetsAuditService);
}
